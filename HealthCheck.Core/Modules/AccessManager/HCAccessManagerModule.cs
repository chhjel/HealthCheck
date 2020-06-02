using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.AccessManager.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace HealthCheck.Core.Modules.AccessManager
{
    /// <summary>
    /// Module for managing HC related access.
    /// </summary>
    public class HCAccessManagerModule : HealthCheckModuleBase<HCAccessManagerModule.AccessOption>
    {
        private HCAccessManagerModuleOptions Options { get; }

        /// <summary>
        /// Module for managing HC related access.
        /// </summary>
        public HCAccessManagerModule(HCAccessManagerModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCAccessManagerModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,

            /// <summary>View generated token data, but not the key itself.</summary>
            ViewToken = 1,

            /// <summary>Create new tokens.</summary>
            CreateNewToken = 2
        }

        /// <summary>
        /// Invoked after request information has been created, and overrides it if an access token was detected.
        /// </summary>
        public HCAccessToken GetTokenForRequest<TAccessRole>(RequestInformation<TAccessRole> currentRequestInformation)
        {
            Guid? tokenId = null;
            try
            {
                var key = "x-token";
                string rawToken = currentRequestInformation.Headers.ContainsKey(key)
        ? currentRequestInformation.Headers[key] : null;

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
                else if (tokenFromStore.ExpiresAt != null && tokenFromStore.ExpiresAt < DateTime.Now) return null;

                var tokenHashBase = CreateBaseForHash(rawToken, tokenFromStore.Roles, tokenFromStore.Modules);
                var isValid = HashUtils.ValidateHash(tokenHashBase, tokenFromStore.HashedToken, tokenFromStore.TokenSalt);
                if (!isValid) return null;

                return tokenFromStore;
            }
            catch (Exception) { }
            return null;
        }
        private static readonly Regex KeyParseRegex = new Regex(@"^KEY-(?<id>[\w]+-[\w]+-[\w]+-[\w]+-[\w]+)-.+");

        #region Invokable methods
        /// <summary>
        /// View all token data.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ViewToken)]
        public object GetTokens()
            => Options.TokenStorage.GetTokens().Select(x => new
            {
                Id = x.Id,
                Name = x.Name
            });

        /// <summary>
        /// Creates a new token.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.CreateNewToken)]
        public object CreateNewToken(HealthCheckModuleContext context, CreateNewTokenRequest data)
        {
            // Filter out any roles not within access of current request
            var requestRoles = EnumUtils.GetFlaggedEnumValues(context.CurrentRequestRoles).Select(x => x.ToString());
            data.Roles = data.Roles.Where(x => requestRoles.Contains(x)).ToList();

            // Filter out any modules and options not within access of current request
            var modules = data.Modules
                .Where(x => context.CurrentRequestModulesAccess.Any(m => m.ModuleId == x.ModuleId))
                .Select(x =>
                {
                    var requestModuleOptions = context.CurrentRequestModulesAccess.FirstOrDefault(m => m.ModuleId == x.ModuleId).AccessOptions;
                    return new HCAccessTokenModuleData()
                    {
                        ModuleId = x.ModuleId,
                        Options = x.Options.Where(o => requestModuleOptions.Contains(o)).ToList()
                    };
                }).ToList();

            var id = Guid.NewGuid();
            var tokenValue = $"KEY-{id}-{Guid.NewGuid()}";
            var tokenHashBase = CreateBaseForHash(tokenValue, data.Roles, modules);
            var tokenHash = HashUtils.GenerateHash(tokenHashBase, out string salt);

            var token = new HCAccessToken
            {
                Id = id,
                Name = data.Name,
                ExpiresAt = data.ExpiresAt,
                HashedToken = tokenHash,
                TokenSalt = salt,
                Roles = data.Roles,
                Modules = modules
            };

            token = Options.TokenStorage.SaveNewToken(token);

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
                    var requestModuleOptions = context.CurrentRequestModulesAccess
                        .FirstOrDefault(m => m.ModuleId == x.Module.GetType().Name).AccessOptions;
                    return new ModuleAccessData()
                    {
                        ModuleName = x.Name,
                        ModuleId = x.Id,
                        AccessOptions = requestModuleOptions
                                .Select(x => new ModuleAccessOption()
                                {
                                    Id = x.ToString(),
                                    Name = x.ToString().SpacifySentence()
                                }).ToList()
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

        #region Private helpers
        private string CreateBaseForHash(string rawToken, List<string> roles, List<HCAccessTokenModuleData> modules)
        {
            var rolesString = string.Join("$", roles);
            var modulesString = string.Join("$", modules.Select(x => $"({x.ModuleId}:{string.Join(",", x.Options)})"));
            return $"{rawToken}|{rolesString}|{modulesString}";
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
            public DateTime? ExpiresAt { get; set; }
            /// <summary></summary>
            public List<string> Roles { get; set; }
            /// <summary></summary>
            public List<CreatedNewTokenRequestModuleData> Modules { get; set; }
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
        }

        private class ModuleAccessData
        {
            public string ModuleName { get; set; }
            public string ModuleId { get; set; }
            public List<ModuleAccessOption> AccessOptions { get; set; }
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
