using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.AccessTokens.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace HealthCheck.Core.Modules.AccessTokens
{
    /// <summary>
    /// Module for managing HC access tokens.
    /// </summary>
    public class HCAccessTokensModule : HealthCheckModuleBase<HCAccessTokensModule.AccessOption>
    {
        private HCAccessTokensModuleOptions Options { get; }

        /// <summary>
        /// Module for managing HC related access.
        /// </summary>
        public HCAccessTokensModule(HCAccessTokensModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.TokenStorage == null) issues.Add("Options.TokenStorage must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCAccessTokensModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>View generated token data, but not the key itself.</summary>
            ViewToken = 1,

            /// <summary>Create new tokens.</summary>
            CreateNewToken = 2,

            /// <summary>Delete tokens.</summary>
            DeleteToken = 4
        }

        /// <summary>
        /// Invoked after request information has been created, and overrides it if an access token was detected.
        /// </summary>
        public HCAccessToken GetTokenForRequest<TAccessRole>(RequestInformation<TAccessRole> currentRequestInformation)
        {
            if (Options.TokenStorage == null) return null;

            Guid? tokenId = null;
            try
            {
                var key = "x-token";
                string rawToken = currentRequestInformation.Headers.ContainsKey(key) ? currentRequestInformation.Headers[key] : null;

                if (rawToken == null)
                {
                    rawToken = HttpUtility.ParseQueryString(new Uri(currentRequestInformation.Url).Query).Get(key);
                }

                if (rawToken != null && KeyParseRegex.IsMatch(rawToken))
                {
                    tokenId = Guid.Parse(KeyParseRegex.Match(rawToken).Groups["id"].Value);
                }

                if (tokenId == null) return null;

                var tokenFromStore = Options.TokenStorage.GetToken(tokenId.Value);
                if (tokenFromStore == null) return null;
                else if (tokenFromStore.ExpiresAt != null && tokenFromStore.ExpiresAt < DateTimeOffset.Now) return null;

                var tokenHashBase = CreateBaseForHash(rawToken, tokenFromStore.Roles, tokenFromStore.Modules, tokenFromStore.ExpiresAt);
                var isValid = HashUtils.ValidateHash(tokenHashBase, tokenFromStore.HashedToken, tokenFromStore.TokenSalt);
                if (!isValid) return null;

                if (tokenFromStore.LastUsedAt == null || (DateTimeOffset.Now - tokenFromStore.LastUsedAt.Value).TotalMinutes >= 1)
                {
                    var updatedToken = Options.TokenStorage.UpdateTokenLastUsedAtTime(tokenFromStore.Id, DateTimeOffset.Now);
                    if (updatedToken != null)
                    {
                        tokenFromStore = updatedToken;
                    }
                }

                return tokenFromStore;
            }
            catch (Exception) {
                return null;
            }
        }

        private static readonly Regex KeyParseRegex = new(@"^KEY-(?<id>[\w]+-[\w]+-[\w]+-[\w]+-[\w]+)-.+");

        #region Invokable methods
        /// <summary>
        /// View all token data.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ViewToken)]
        public object GetTokens()
        {
            return Options.TokenStorage?.GetTokens()
                ?.OrderByDescending(x => x.CreatedAt)
                ?.Select(x =>
                {
                    string expirationSummary = null;
                    if (x.ExpiresAt != null)
                    {
                        var timeUntil = (long)(x.ExpiresAt.Value - DateTimeOffset.Now).TotalMilliseconds;
                        expirationSummary = (timeUntil < 0)
                            ? "Expired"
                            : $"Expires in {TimeUtils.PrettifyDuration(timeUntil)}";
                    }

                    string lastUsedSummary = null;
                    if (x.LastUsedAt != null)
                    {
                        lastUsedSummary = $"Last used {TimeUtils.PrettifyDurationSince(x.LastUsedAt, TimeSpan.FromMinutes(1), "less than a minute")} ago";
                    }
                    else
                    {
                        lastUsedSummary = "Not used yet";
                    }

                    string createdSummary = $"Created {TimeUtils.PrettifyDurationSince(x.CreatedAt, TimeSpan.FromMinutes(1), "less than a minute")} ago";

                    foreach(var module in x.Modules)
                    {
                        module.Categories ??= new List<string>();
                    }

                    return new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CreatedAt = x.CreatedAt,
                        CreatedAtSummary = createdSummary,
                        LastUsedAt = x.LastUsedAt,
                        LastUsedAtSummary = lastUsedSummary,
                        ExpiresAt = x.ExpiresAt,
                        ExpiresAtSummary = expirationSummary,
                        IsExpired = (x.ExpiresAt != null && x.ExpiresAt.Value < DateTimeOffset.Now),
                        Roles = x.Roles,
                        Modules = x.Modules,
                        AllowKillswitch = x.AllowKillswitch
                    };
                });
        }

        /// <summary>
        /// View all token data.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.DeleteToken)]
        public object DeleteToken(HealthCheckModuleContext context, Guid id)
        {
            var token = Options.TokenStorage.GetToken(id);
            if (token != null)
            {
                context.AddAuditEvent(action: "Access token deleted", subject: token.Name)
                    .AddDetail("Token id", token.Id.ToString());
            }

            Options.TokenStorage.DeleteToken(id);
            return new { Success = true };
        }

        /// <summary>
        /// Creates a new token.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.CreateNewToken)]
        public object CreateNewToken(HealthCheckModuleContext context, CreateNewTokenRequest data)
        {
            if (Options.TokenStorage == null) return null;

            // Filter out any roles not within access of current request
            var requestRoles = EnumUtils.GetFlaggedEnumValues(context.CurrentRequestRoles).Select(x => x.ToString());
            data.Roles = data.Roles.Where(x => requestRoles.Contains(x)).ToList();

            // Filter out any modules and options not within access of current request
            var modules = data.Modules
                .Where(x => context.CurrentRequestModulesAccess.Any(m => m.ModuleId == x.ModuleId))
                .Select(x =>
                {
                    var moduleAccess = context.CurrentRequestModulesAccess.FirstOrDefault(m => m.ModuleId == x.ModuleId);
                    return new HCAccessTokenModuleData()
                    {
                        ModuleId = x.ModuleId,
                        Options = x.Options.Where(o => moduleAccess.AccessOptions.Contains(o)).ToList(),
                        Categories = x.Categories.Where(o => moduleAccess.AccessCategories?.Any() != true || moduleAccess.AccessCategories.Contains(o)).ToList(),
                        Ids = x.Ids.Where(o => moduleAccess.AccessIds?.Any() != true || moduleAccess.AccessIds.Contains(o)).ToList()
                    };
                }).ToList();

            var id = Guid.NewGuid();
            var tokenValue = $"KEY-{id}-{Guid.NewGuid()}";
            var tokenHashBase = CreateBaseForHash(tokenValue, data.Roles, modules, data.ExpiresAt);
            var tokenHash = HashUtils.GenerateHash(tokenHashBase, out string salt);

            var token = new HCAccessToken
            {
                Id = id,
                Name = data.Name,
                CreatedAt = DateTimeOffset.Now,
                ExpiresAt = data.ExpiresAt,
                HashedToken = tokenHash,
                TokenSalt = salt,
                Roles = data.Roles,
                Modules = modules,
                AllowKillswitch = data.AllowKillswitch
            };

            token = Options.TokenStorage.SaveNewToken(token);
            context.AddAuditEvent(action: "Access token created", subject: token.Name)
                .AddDetail("Token id", token.Id.ToString());

            return new
            {
                Id = token.Id,
                Name = token.Name,
                Token = tokenValue
            };
        }

        /// <summary>
        /// Get overview of roles and module options.
        /// <para>Only items the request has access to will be included.</para>
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.CreateNewToken)]
        public object GetAccessData(HealthCheckModuleContext context)
        {
            var allRoles = EnumUtils.GetFlaggedEnumValues(context.CurrentRequestRoles)
                .Where(x => (int)Convert.ChangeType(x, typeof(int)) > 0)
                .Select(x => new Role()
                {
                    Id = x.ToString(),
                    Name = x.ToString().SpacifySentence()
                });

            var moduleOptions = context.LoadedModules
                .Where(x => context.CurrentRequestModulesAccess.Any(m => m.ModuleId == x.Module.GetType().Name))
                .Select(x => {
                    var moduleAccess = context.CurrentRequestModulesAccess
                        .FirstOrDefault(m => m.ModuleId == x.Module.GetType().Name);
                    var requestModuleOptions = moduleAccess.AccessOptions;
                    var requestModuleCategories = moduleAccess.AccessCategories;
                    var requestModuleIds = new HashSet<string>(moduleAccess.AccessIds);

                    List<string> categories;
                    if (requestModuleCategories?.Any() == true)
                    {
                        categories = requestModuleCategories ?? new List<string>();
                    }
                    else
                    {
                        categories = x.AllModuleCategories ?? new List<string>();
                    }

                    List<HCModuleIdData> ids;
                    if (requestModuleIds?.Any() == true)
                    {
                        ids = x.AllModuleIds?.Where(mid => requestModuleIds?.Contains(mid.Id) == true)?.ToList() ?? new List<HCModuleIdData>();
                    }
                    else
                    {
                        ids = x.AllModuleIds ?? new List<HCModuleIdData>();
                    }

                    return new ModuleAccessData()
                    {
                        ModuleName = x.Name,
                        ModuleId = x.Id,
                        AccessOptions = requestModuleOptions
                                .Select(x => new ModuleAccessOption()
                                {
                                    Id = x.ToString(),
                                    Name = x.ToString().SpacifySentence()
                                }).ToList(),
                        AccessCategories = categories
                                .Select(x => new ModuleAccessOption()
                                {
                                    Id = x.ToString(),
                                    Name = x.ToString().SpacifySentence()
                                })
                                .OrderBy(x => x.Id)
                                .ToList(),
                        AccessIds = ids
                                .OrderBy(x => x.Name)
                                .ToList()
                    };
                })
                .ToList();

            return new
            {
                Roles = allRoles,
                ModuleOptions = moduleOptions
            };
        }
        #endregion

        #region Actions
        private const string Q = "\"";
        private static readonly Regex TokenKillswitchUrlRegex
            = new(@"^/ATTokenKillswitch/kill", RegexOptions.IgnoreCase);

        /// <summary>
        /// Killswitch for the currently used token.
        /// </summary>
        [HealthCheckModuleAction]
        public object ATTokenKillswitch(HealthCheckModuleContext context, string url)
        {
            if (context?.Request?.IsPOST != true)
            {
                return null;
            }
            var match = TokenKillswitchUrlRegex.Match(url);
            if (!match.Success)
            {
                return null;
            }
            else if (context?.IsUsingAccessToken != true)
            {
                return null;
            }

            // Get current token
            var token = Options.TokenStorage.GetToken(context.CurrentTokenId.Value);
            if (token == null)
            {
                return null;
            }
            else if (!token.AllowKillswitch)
            {
                return createResult(false);
            }

            // Audit log and delete
            context.AddAuditEvent(action: "Access token killswitched", subject: token.Name)
                .AddDetail("Token id", token.Id.ToString());
            Options.TokenStorage.DeleteToken(token.Id);

            return createResult(true);

            static string createResult(bool success)
            {
                return
                    "{\n" +
                    $"  {Q}success{Q}: {success.ToString().ToLower()}\n" +
                    "}";
            }
        }
        #endregion

        #region Private helpers
        private string CreateBaseForHash(string rawToken, List<string> roles, List<HCAccessTokenModuleData> modules, DateTimeOffset? expiresAt)
        {
            var rolesString = string.Join("$", roles);
            var modulesString = string.Join("$", modules.Select(x =>
            {
                var categoryPart = "";
                if (x.Categories?.Any() == true)
                {
                    categoryPart = $":[{string.Join(",", x.Categories)}]";
                }
                var idsPart = "";
                if (x.Ids?.Any() == true)
                {
                    idsPart = $":[{string.Join(",", x.Ids)}]";
                }
                return $"({x.ModuleId}:{string.Join(",", x.Options)}{categoryPart}{idsPart})";
            }));
            var expirationString = (expiresAt == null) ? "no-expiration" : expiresAt.Value.Ticks.ToString();
            return $"{rawToken}|{rolesString}|{modulesString}|{expirationString}";
        }
        #endregion

        #region Models
        /// <summary>
        /// Model sent to endpoint to create a new token.
        /// </summary>
        public class CreateNewTokenRequest
        {
            /// <summary></summary>
            public string Name { get; set; }
            /// <summary></summary>
            public DateTimeOffset? ExpiresAt { get; set; }
            /// <summary></summary>
            public List<string> Roles { get; set; }
            /// <summary></summary>
            public List<CreatedNewTokenRequestModuleData> Modules { get; set; }
            /// <summary></summary>
            public bool AllowKillswitch { get; set; }
        }

        /// <summary>
        /// Sub-model sent to endpoint to create a new token.
        /// </summary>
        public class CreatedNewTokenRequestModuleData
        {
            /// <summary></summary>
            public string ModuleId { get; set; }
            /// <summary></summary>
            public List<string> Options { get; set; }
            /// <summary></summary>
            public List<string> Categories { get; set; }
            /// <summary></summary>
            public List<string> Ids { get; set; }
        }

        private class ModuleAccessData
        {
            public string ModuleName { get; set; }
            public string ModuleId { get; set; }
            public List<ModuleAccessOption> AccessOptions { get; set; }
            public List<ModuleAccessOption> AccessCategories { get; set; }
            public List<HCModuleIdData> AccessIds { get; set; }
        }
        private class ModuleAccessOption
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        private class Role
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        #endregion
    }
}
