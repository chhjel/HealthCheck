﻿using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace HealthCheck.Core.Modules.SecureFileDownload
{
    /// <summary>
    /// Module for downloading files a bit more securely.
    /// </summary>
    public class HCSecureFileDownloadModule : HealthCheckModuleBase<HCSecureFileDownloadModule.AccessOption>
    {
        private HCSecureFileDownloadModuleOptions Options { get; }

        private const string Q = "\"";
        private static ListWithExpiration<CachedToken> _tokenCache = new ListWithExpiration<CachedToken>();
        private struct CachedToken
        {
            public string Token;
            public Guid DefinitionId;
        }

        /// <summary>
        /// Module for downloading files a bit more securely.
        /// </summary>
        public HCSecureFileDownloadModule(HCSecureFileDownloadModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.DefinitionStorage == null) issues.Add($"Options.{nameof(Options.DefinitionStorage)} must be set.");
            if (Options.FileStorages?.Any() != true) issues.Add($"Options.{nameof(Options.FileStorages)} must contain at least one storage.");

            var duplicateStorageIds = Options.FileStorages
                ?.GroupBy(x => x.StorageId)
                ?.Where(x => x.Count() > 1)
                ?.Select(x => x.Key)
                ?? Enumerable.Empty<string>();
            foreach(var dupeId in duplicateStorageIds)
            {
                issues.Add($"Options.{nameof(Options.FileStorages)} contains multiple implementations with the same id '{dupeId}'.");
            }
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCSecureFileDownloadModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>Create new definitions.</summary>
            CreateDefinition = 1,

            /// <summary>Edit existing definitions.</summary>
            EditDefinition = 2,

            /// <summary>Delete definitions.</summary>
            DeleteDefinition = 4,

            /// <summary>View all created definitions.</summary>
            ViewDefinitions = 8
        }

        #region Invokable methods
        /// <summary>
        /// Get all download definitions.
        /// </summary>
        [HealthCheckModuleMethod(requiresAccessTo: AccessOption.ViewDefinitions)]
        public SecureFileDownloadsViewModel GetDownloads()
        {
            var definitions = Options.DefinitionStorage.GetDefinitions()
                .Select(x =>
                {
                    x.Password = null;
                    return x;
                });

            return new SecureFileDownloadsViewModel
            {
                Definitions = definitions
            };
        }

        /// <summary>
        /// Create a new definition.
        /// </summary>
        [HealthCheckModuleMethod(requiresAccessTo: AccessOption.CreateDefinition)]
        public SecureFileDownloadDefinition CreateDefinition(HealthCheckModuleContext context, SecureFileDownloadDefinition definition)
        {
            definition.CreatedAt = definition.LastModifiedAt = DateTimeOffset.Now;
            definition.CreatedByUsername = definition.LastModifiedByUsername = context.UserName;
            definition.CreatedByUserId = definition.LastModifiedByUserId = context.UserId;

            context.AddAuditEvent("Create download", definition.FileName);
            return Options.DefinitionStorage.CreateDefinition(definition);
        }

        /// <summary>
        /// Update a stored definition.
        /// </summary>
        [HealthCheckModuleMethod(requiresAccessTo: AccessOption.EditDefinition)]
        public SecureFileDownloadDefinition UpdateDefinition(HealthCheckModuleContext context, SecureFileDownloadDefinition definition)
        {
            definition.LastModifiedAt = DateTimeOffset.Now;
            definition.LastModifiedByUsername = context.UserName;
            definition.LastModifiedByUserId = context.UserId;

            context.AddAuditEvent("Update download", definition.FileName);
            return Options.DefinitionStorage.UpdateDefinition(definition);
        }

        /// <summary>
        /// Update a stored definition.
        /// </summary>
        [HealthCheckModuleMethod(requiresAccessTo: AccessOption.DeleteDefinition)]
        public bool DeleteDefinition(HealthCheckModuleContext context, Guid id)
        {
            var definition = Options.DefinitionStorage.GetDefinition(id);
            if (definition == null)
            {
                return false;
            }

            context.AddAuditEvent("Remove download", definition.FileName);
            Options.DefinitionStorage.DeleteDefinition(id);
            return true;
        }
        #endregion

        #region Actions
        private static readonly Regex DownloadUrlRegex
            = new Regex(@"^/Download/(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);
        private static readonly Regex DownloadFileUrlRegex
            = new Regex(@"^/SFDDownloadFile/(?<token>[\w-]+)?__(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);

        /// <summary>
        /// Show download page for a file.
        /// <para>Also handles POST request with password.</para>
        /// </summary>
        [HealthCheckModuleAction]
        public object Download(HealthCheckModuleContext context, string url)
        {
            var match = DownloadUrlRegex.Match(url);
            if (!match.Success)
            {
                return null;
            }

            // Parse url
            var idString = match.Groups["id"].Value.Trim().ToLower();

            // Get definition
            var definition = Options.DefinitionStorage.GetDefinitionByUrlSegmentText(idString);
            if (definition == null)
            {
                return null;
            }

            // Find matching storage
            var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == definition.StorageId);
            if (storage == null)
            {
                return null;
            }

            // Check that download is not expired etc
            string definitionValidationError = ValidateDownload(definition, storage);

            return CreateDownloadPageHtml(context, definition, definitionValidationError);
        }

        /// <summary>
        /// POST. Validate password and get a token in return.
        /// </summary>
        [HealthCheckModuleAction]
        public string SFDValidatePassword(HealthCheckModuleContext context)
        {
            if (context?.Request?.IsPOST != true)
            {
                return null;
            }

            if (!context.Request.Headers.ContainsKey("x-id")
                || !context.Request.Headers.ContainsKey("x-pwd"))
            {
                return null;
            }

            var idFromRequest = context.Request.Headers["x-id"];
            var passwordFromRequest = context.Request.Headers["x-pwd"];

            // Get definition
            var definition = Options.DefinitionStorage.GetDefinitionByUrlSegmentText(idFromRequest);
            if (definition == null)
            {
                return null;
            }

            // Find matching storage
            var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == definition.StorageId);
            if (storage == null)
            {
                return null;
            }

            // Abort if there's no password required for this one.
            if (string.IsNullOrWhiteSpace(definition.Password))
            {
                return null;
            }

            string createResult(bool success, string message, string token)
            {
                return
                    "{\n" +
                    $"  {Q}success{Q}: {success.ToString().ToLower()},\n" +
                    $"  {Q}message{Q}: {EscapeJsString(message)},\n" +
                    $"  {Q}token{Q}: {EscapeJsString(token)}\n" +
                    "}";
            }

            // Check that download is not expired etc
            string definitionValidationError = ValidateDownload(definition, storage);
            if (definitionValidationError != null)
            {
                return createResult(false, definitionValidationError, null);
            }

            // Validate password && create token if valid
            string token = null;
            if (passwordFromRequest != definition.Password)
            {
                return createResult(false, "Invalid password", null);
            }

            token = CreateToken();
            _tokenCache.Add(new CachedToken
            {
                Token = token,
                DefinitionId = definition.Id
            },
            expiresAt: DateTime.Now + Options.DownloadTokenLifetime);

            return createResult(true, null, token);
        }

        /// <summary>
        /// Download a file, or show download page if not allowed.
        /// </summary>
        [HealthCheckModuleAction]
        public object SFDDownloadFile(HealthCheckModuleContext context, string url)
        {
            var match = DownloadFileUrlRegex.Match(url);
            if (!match.Success)
            {
                return null;
            }

            // Parse url
            var idFromUrl = match.Groups["id"].Value.Trim().ToLower();
            var tokenUrlMatch = match.Groups["token"];
            string tokenFromUrl = tokenUrlMatch.Success ? tokenUrlMatch.Value : null;

            // Get definition
            var definition = Options.DefinitionStorage.GetDefinitionByUrlSegmentText(idFromUrl);
            if (definition == null)
            {
                return null;
            }

            // Find matching storage
            var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == definition.StorageId);
            if (storage == null)
            {
                return null;
            }

            // Check that download is not expired etc
            string definitionValidationError = ValidateDownload(definition, storage);
            if (definitionValidationError != null)
            {
                return CreateDownloadPageHtml(context, definition, definitionValidationError);
            }

            var downloadRequiresPassword = !string.IsNullOrWhiteSpace(definition.Password);
            if (downloadRequiresPassword)
            {
                if (!_tokenCache.Any(x => x.Token == tokenFromUrl && x.DefinitionId == definition.Id))
                {
                    definitionValidationError = "Invalid or expired token.";
                    return CreateDownloadPageHtml(context, definition, definitionValidationError);
                }

                _tokenCache.RemoveWhere(x => x.Token == tokenFromUrl);
                _tokenCache.RemoveExpired();
            }

            // Get file stream from stored file id
            var fileStream = storage.GetFileStream(definition.FileId);

            // Increment download count
            definition.DownloadCount++;
            definition.LastDownloadedAt = DateTimeOffset.Now;
            definition = Options.DefinitionStorage.UpdateDefinition(definition);

            // Store audit data
            context.AddAuditEvent("Secure file download", definition.FileName)
                .AddClientConnectionDetails(context)
                .AddDetail("File Id", definition.FileId)
                .AddDetail("Storage Id", definition.StorageId);

            return new HealthCheckFileStreamResult
            {
                FileName = definition.FileName,
                ContentStream = fileStream
            };
        }
        #endregion

        #region Helpers
        private static string ValidateDownload(SecureFileDownloadDefinition definition, ISecureFileDownloadFileStorage storage)
        {
            string definitionValidationError = null;
            if (definition.ExpiresAt != null && definition.ExpiresAt < DateTimeOffset.Now)
            {
                definitionValidationError = $"The download link expired at {definition.ExpiresAt}.";
            }
            else if (definition.DownloadCountLimit != null && definition.DownloadCount >= definition.DownloadCountLimit)
            {
                definitionValidationError = "The download link has expired, max download count reached.";
            }
            else if (!storage.HasFile(definition.FileId))
            {
                definitionValidationError = "The file was not found.";
            }

            return definitionValidationError;
        }

        private string CreateToken() => $"{Guid.NewGuid()}-{Guid.NewGuid()}";

        private string EscapeJsString(string value, bool addQuotes = true)
            => (value == null) ? "null" : HttpUtility.JavaScriptStringEncode(value, addQuotes);

        /// <summary>
        /// Create the html to show for the download file page when not downloading directly.
        /// </summary>
        protected virtual string CreateDownloadPageHtml(
            HealthCheckModuleContext context, SecureFileDownloadDefinition definition,
            string definitionValidationError)
        {
            var javascriptUrlTags = context.JavaScriptUrls
                .Select(url => $"<script src=\"{url}\"></script>")
                .ToList();
            var javascriptUrlTagsHtml = string.Join("\n    ", javascriptUrlTags);

            var defaultAssets = $@"
    <link href={Q}https://cdn.jsdelivr.net/npm/vuetify@1.5.6/dist/vuetify.min.css{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Montserrat{Q} rel={Q}stylesheet{Q}>
    <link href={Q}https://use.fontawesome.com/releases/v5.7.2/css/all.css{Q} rel={Q}stylesheet{Q} integrity={Q}sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr{Q} crossorigin={Q}anonymous{Q}>";

            var noIndexMeta = $"<meta name={Q}robots{Q} content={Q}noindex{Q}>";

            var expiresAt = "";
            if (definition.ExpiresAt != null)
            {
                expiresAt = definition.ExpiresAt.Value.ToUnixTimeSeconds().ToString();
            }

            var downloadsRemaining = "";
            if (definition.DownloadCountLimit != null)
            {
                downloadsRemaining = Math.Max(0, definition.DownloadCountLimit.Value - definition.DownloadCount).ToString();
            }

            var requiresPassword = !string.IsNullOrWhiteSpace(definition.Password);
            var downloadLink = $"SFDDownloadFile/__{definition.UrlSegmentText}";

            var title = Options.DownloadPageTitle ?? "";
            title = title.Replace("[FILENAME]", definition.FileName ?? "unknown");

            return $@"
<!doctype html>
<html>
<head>
    <title>{title}</title>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    {defaultAssets}
</head>

<body>
    <div id={Q}app-download{Q}></div>

    <script>
        window.__hc_data = {{
            definitionValidationError: {EscapeJsString(definitionValidationError)},
            download: {{
                name: {EscapeJsString(definition.UrlSegmentText)},
                filename: {EscapeJsString(definition.FileName)},
                downloadLink: {EscapeJsString(downloadLink)},
                protected: {requiresPassword.ToString().ToLower()},
                expiresAt: {EscapeJsString(expiresAt)},
                downloadsRemaining: {EscapeJsString(downloadsRemaining)},
                note: {EscapeJsString(definition.Note)},
            }}
        }};
    </script>
    {javascriptUrlTagsHtml}
</body>
</html>";
        }
        #endregion
    }
}
