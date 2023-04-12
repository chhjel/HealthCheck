using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;
using QoDL.Toolkit.Core.Util.Collections;
using QoDL.Toolkit.Core.Util.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.SecureFileDownload;

/// <summary>
/// Module for downloading files a bit more securely.
/// </summary>
public class TKSecureFileDownloadModule : ToolkitModuleBase<TKSecureFileDownloadModule.AccessOption>
{
    private TKSecureFileDownloadModuleOptions Options { get; }

    private const string Q = "\"";
    private static readonly ListWithExpiration<CachedToken> _tokenCache = new();
    private struct CachedToken
    {
        public string Token;
        public Guid DefinitionId;
    }

    /// <summary>
    /// Module for downloading files a bit more securely.
    /// </summary>
    public TKSecureFileDownloadModule(TKSecureFileDownloadModuleOptions options)
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
        foreach (var dupeId in duplicateStorageIds)
        {
            issues.Add($"Options.{nameof(Options.FileStorages)} contains multiple implementations with the same id '{dupeId}'.");
        }
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context)
    {
        var model = new SecureFileDownloadFrontendOptionsModel();
        foreach (var storage in Options.FileStorages)
        {
            model.StorageInfos.Add(new SecureFileDownloadStorageInfo()
            {
                StorageId = storage.StorageId,
                StorageName = storage.StorageName,
                FileIdInfo = storage.FileIdInfo,
                FileIdLabel = storage.FileIdLabel,
                SupportsUpload = storage.SupportsUpload,
                SupportsSelectingFile = storage.SupportsSelectingFile
            });
        }
        return model;
    }

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKSecureFileDownloadModuleConfig();

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
        ViewDefinitions = 8,

        /// <summary>Upload new files.</summary>
        UploadFile = 16
    }

    #region Invokable methods
    /// <summary>
    /// Get all download definitions.
    /// </summary>
    [ToolkitModuleMethod(requiresAccessTo: AccessOption.ViewDefinitions)]
    public SecureFileDownloadsViewModel GetDownloads()
    {
        var definitions = Options.DefinitionStorage.GetDefinitions();

        return new SecureFileDownloadsViewModel
        {
            Definitions = definitions
        };
    }

    /// <summary>
    /// Get fileId options for a given storage.
    /// </summary>
    [ToolkitModuleMethod(requiresAccessTo: AccessOption.CreateDefinition)]
    public List<TKSecureFileDownloadFileDetails> GetStorageFileIdOptions(string storageId)
    {
        var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == storageId);
        if (storage?.SupportsSelectingFile != true) return new List<TKSecureFileDownloadFileDetails>();
        return storage?.GetFileIdOptions()?.ToList() ?? new List<TKSecureFileDownloadFileDetails>();
    }

    /// <summary>
    /// Create or update a definition.
    /// </summary>
    [ToolkitModuleMethod(requiresAccessTo: AccessOption.CreateDefinition)]
    public SecureFileDownloadSaveViewModel SaveDefinition(ToolkitModuleContext context, SecureFileDownloadDefinition definition)
    {
        definition.UrlSegmentText = definition.UrlSegmentText.Trim();

        var existing = Options.DefinitionStorage.GetDefinition(definition.Id);
        var isNew = existing == null;
        if (isNew)
        {
            definition.CreatedAt = DateTimeOffset.Now;
            definition.CreatedByUsername = context.UserName;
            definition.CreatedByUserId = context.UserId;
            context.AddAuditEvent("Create download", definition.FileName)
                .AddDetail("File Name", definition.FileName)
                .AddDetail("File Id", definition.FileId)
                .AddDetail("Storage Id", definition.StorageId);
        }
        else
        {
            definition.CreatedAt = existing.CreatedAt;
            definition.CreatedByUsername = existing.CreatedByUsername;
            definition.CreatedByUserId = existing.CreatedByUserId;
            context.AddAuditEvent("Edit download", definition.FileName)
                .AddDetail("File Name", definition.FileName)
                .AddDetail("File Id", definition.FileId)
                .AddDetail("Storage Id", definition.StorageId);
        }

        var validationError = ValidateDefinitionBeforeSave(definition, isNew, existing?.UrlSegmentText);
        if (validationError != null)
        {
            return new SecureFileDownloadSaveViewModel()
            {
                Success = false,
                ErrorMessage = validationError
            };
        }

        definition.LastModifiedAt = DateTimeOffset.Now;
        definition.LastModifiedByUsername = context.UserName;
        definition.LastModifiedByUserId = context.UserId;

        if (isNew)
        {
            definition = Options.DefinitionStorage.CreateDefinition(definition);
        }
        else
        {
            definition = Options.DefinitionStorage.UpdateDefinition(definition);
        }

        return new SecureFileDownloadSaveViewModel()
        {
            Success = true,
            Definition = definition
        };
    }

    /// <summary>
    /// Update a stored definition.
    /// </summary>
    [ToolkitModuleMethod(requiresAccessTo: AccessOption.DeleteDefinition)]
    public async Task<bool> DeleteDefinition(ToolkitModuleContext context, Guid id)
    {
        var definition = Options.DefinitionStorage.GetDefinition(id);
        if (definition == null)
        {
            return false;
        }

        context.AddAuditEvent("Remove download", definition.FileName)
            .AddDetail("File Name", definition.FileName)
            .AddDetail("File Id", definition.FileId)
            .AddDetail("Storage Id", definition.StorageId);

        Options.DefinitionStorage.DeleteDefinition(id);

        if (definition.HasUploadedFile && !string.IsNullOrWhiteSpace(definition.FileId))
        {
            var storageId = definition.StorageId;
            var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == storageId);
            if (storage != null)
            {
                await storage.DeleteUploadedFileAsync(definition.FileId);
            }
        }

        return true;
    }

    /// <summary>
    /// Deletes uploaded file for definition.
    /// </summary>
    [ToolkitModuleMethod(requiresAccessTo: AccessOption.EditDefinition)]
    public async Task<bool> DeleteUploadedFile(ToolkitModuleContext context, Guid id)
    {
        var definition = Options.DefinitionStorage.GetDefinition(id);
        if (definition == null)
        {
            return false;
        }

        var storageId = definition.StorageId;
        var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == storageId);
        if (storage == null)
        {
            return false;
        }

        var isRemoved = await storage.DeleteUploadedFileAsync(definition.FileId);
        if (!isRemoved) return false;

        context.AddAuditEvent("Deleted uploaded file", definition.FileName)
            .AddDetail("File Name", definition.FileName)
            .AddDetail("Deleted file name", definition.OriginalFileName)
            .AddDetail("Storage Id", definition.StorageId);

        definition.FileId = "";
        definition.OriginalFileName = "";
        definition.HasUploadedFile = false;
        definition = Options.DefinitionStorage.UpdateDefinition(definition);

        return true;
    }
    #endregion

    #region Actions
    private static readonly Regex DownloadUrlRegex
        = new(@"^/Download/(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);
    private static readonly Regex DownloadFileUrlRegex
        = new(@"^/SFDDownloadFile/(?<token>[\w-]+)?__(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);

    /// <summary>
    /// Show download page for a file.
    /// </summary>
    [ToolkitModuleAction]
    public object Download(ToolkitModuleContext context, string url)
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
    [ToolkitModuleAction]
    public async Task<string> SFDValidatePassword(ToolkitModuleContext context)
    {
        if (context?.Request?.IsPOST != true)
        {
            return null;
        }

        // lazy brute force delay
        await Task.Delay(3000).ConfigureAwait(false);

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
        if (string.IsNullOrEmpty(definition.Password))
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
    [ToolkitModuleAction]
    public object SFDDownloadFile(ToolkitModuleContext context, string url)
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

        var downloadRequiresPassword = !string.IsNullOrEmpty(definition.Password);
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
        context.AddAuditEvent("File download", definition.FileName)
            .AddClientConnectionDetails(context)
            .AddDetail("File Name", definition.FileName)
            .AddDetail("File Id", definition.FileId)
            .AddDetail("Storage Id", definition.StorageId);

        return ToolkitFileDownloadResult.CreateFromStream(definition.FileName, fileStream);
    }

    /// <summary>
    /// POST. Upload a new file to be stored.
    /// </summary>
    [ToolkitModuleAction(AccessOption.UploadFile)]
    public async Task<string> SFDUploadFile(ToolkitModuleContext context)
    {
        if (context?.Request?.IsPOST != true || context?.Request?.InputStream == null)
        {
            return null;
        }

        if (!context.Request.Headers.ContainsKey("x-id"))
        {
            return null;
        }

        var idFromRequestRaw = context.Request.Headers["x-id"];
        if (!Guid.TryParse(idFromRequestRaw, out var idFromRequest))
        {
            return null;
        }

        var storageId = context.Request.Headers["x-storage-id"];
        if (string.IsNullOrWhiteSpace(storageId))
        {
            return null;
        }

        var file = context?.Request?.FormFiles?.FirstOrDefault();
        var streamGetter = file?.GetStream;
        if (streamGetter == null)
        {
            return null;
        }

        // Get definition
        var definition = Options.DefinitionStorage.GetDefinition(idFromRequest);
        if (definition == null)
        {
            return null;
        }

        // Find matching storage
        var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == storageId);
        if (storage?.SupportsUpload != true)
        {
            return null;
        }

        // If any existing file, delete it
        if (!string.IsNullOrWhiteSpace(definition.FileId) && definition.HasUploadedFile)
        {
            var isRemoved = await storage.DeleteUploadedFileAsync(definition.FileId).ConfigureAwait(false);
            if (!isRemoved)
            {
                return createResult(false, "Failed to delete existing file.", "");
            }
            definition.FileId = "";
            definition.OriginalFileName = "";
            definition.HasUploadedFile = false;
            definition = Options.DefinitionStorage.UpdateDefinition(definition);
        }

        // Upload file
        var stream = streamGetter();
        var uploadResult = await storage.UploadFileAsync(stream).ConfigureAwait(false);

        if (uploadResult.Success)
        {
            definition.FileId = uploadResult.FileId;
            definition.OriginalFileName = file?.FileName ?? "UnnamedFile";
            definition.HasUploadedFile = true;
            definition.StorageId = storageId;
            definition = Options.DefinitionStorage.UpdateDefinition(definition);

            context.AddAuditEvent("Uploaded file", definition.FileName)
                .AddDetail("File Name", definition.FileName)
                .AddDetail("Original file Name", definition.OriginalFileName)
                .AddDetail("File Id", definition.FileId)
                .AddDetail("Storage Id", definition.StorageId);
        }

        return createResult(uploadResult.Success, uploadResult.ErrorMessage, uploadResult.FileId);

        string createResult(bool success, string message, string fileId)
        {
            return
                "{\n" +
                $"  {Q}success{Q}: {success.ToString().ToLower()},\n" +
                $"  {Q}message{Q}: {EscapeJsString(message)},\n" +
                $"  {Q}fileId{Q}: {EscapeJsString(fileId)},\n" +
                $"  {Q}defId{Q}: {EscapeJsString(definition.Id.ToString())}\n" +
                "}";
        }
    }
    #endregion

    #region Helpers
    private string ValidateDefinitionBeforeSave(SecureFileDownloadDefinition definition,
        bool isNew, string existingUrlText)
    {
        if (!string.IsNullOrEmpty(definition.Password) && definition.Password.Length < 6)
        {
            return "Password must be at least 6 characters long.";
        }
        else if (string.IsNullOrWhiteSpace(definition.FileName.Trim()))
        {
            return "A filename must be set.";
        }
        else if (string.IsNullOrWhiteSpace(definition.UrlSegmentText))
        {
            return "A text for the url must be set.";
        }

        var storage = Options.FileStorages.FirstOrDefault(x => x.StorageId == definition.StorageId);
        if (storage == null)
        {
            return $"No storage with the id '{definition.StorageId}' was found.";
        }

        var storageFileIdValidation = storage.ValidateFileIdBeforeSave(definition.FileId);
        if (!string.IsNullOrWhiteSpace(storageFileIdValidation))
        {
            return storageFileIdValidation;
        }

        // Prevent creating new with same url text as an existing one
        if (isNew
            && Options.DefinitionStorage.GetDefinitionByUrlSegmentText(definition.UrlSegmentText.Trim()) != null)
        {
            return "There is already another download with the same url text.";
        }
        // Prevent changing existing to same url text as an existing one
        else if (!isNew
            && existingUrlText.ToLower() != definition.UrlSegmentText.ToLower()
            && Options.DefinitionStorage.GetDefinitionByUrlSegmentText(definition.UrlSegmentText.Trim()) != null)
        {
            return "There is already another download with the same url text.";
        }
        return null;
    }

    private static string ValidateDownload(SecureFileDownloadDefinition definition, ISecureFileDownloadFileStorage storage)
    {
        string definitionValidationError = null;
        if (definition.IsExpired)
        {
            definitionValidationError = $"The download link expired at {definition.ExpiresAt}.";
        }
        else if (definition.DownloadCountLimit != null && definition.DownloadCount >= definition.DownloadCountLimit)
        {
            definitionValidationError = "The download link has expired, download count limit reached.";
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
        ToolkitModuleContext context, SecureFileDownloadDefinition definition,
        string definitionValidationError)
    {
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

        var requiresPassword = !string.IsNullOrEmpty(definition.Password);
        var downloadLink = $"SFDDownloadFile/__{definition.UrlSegmentText}";

        var title = Options.DownloadPageTitle ?? "";
        title = title.Replace("[FILENAME]", definition.FileName ?? "unknown");

        var cssTagsHtml = TKAssetGlobalConfig.CreateCssTags(context.CssUrls);
        var jsTagsHtml = TKAssetGlobalConfig.CreateJavaScriptTags(context.JavaScriptUrls);

        return $@"
<!doctype html>
<html>
<head>
    <title>{title}</title>
    <meta charset={Q}utf-8{Q}>
    <meta name={Q}robots{Q} content={Q}noindex{Q}>
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    {cssTagsHtml}
</head>

<body>
    <div id={Q}app-download{Q}></div>

    <script>
        window.__tk_data = {{
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
    {jsTagsHtml}
</body>
</html>";
    }
    #endregion
}
