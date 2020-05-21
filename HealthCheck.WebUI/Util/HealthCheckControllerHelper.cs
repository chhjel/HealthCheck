using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Exceptions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Serializers;
using HealthCheck.WebUI.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HealthCheck.WebUI.Util
{
    /// <summary>
    /// Shared code for .net framework/core controllers.
    /// </summary>
    internal class HealthCheckControllerHelper<TAccessRole>
    {
        /// <summary>
        /// Initialize a new HealthCheck helper with the given services.
        /// </summary>
        public HealthCheckControllerHelper(HealthCheckServiceContainer<TAccessRole> serviceContainer)
        {
            Services = serviceContainer ?? new HealthCheckServiceContainer<TAccessRole>();
        }

        /// <summary>
        /// Contains services that enables extra functionality.
        /// </summary>
        public HealthCheckServiceContainer<TAccessRole> Services { get; } = new HealthCheckServiceContainer<TAccessRole>();

        /// <summary>
        /// Access related options.
        /// </summary>
        public AccessOptions<TAccessRole> AccessOptions { get; set; } = new AccessOptions<TAccessRole>();

        internal bool HasAccessToAnyContent(Maybe<TAccessRole> currentRequestAccessRoles)
            => GetModulesRequestHasAccessTo(currentRequestAccessRoles).Count > 0;

        #region Modules
        private List<HealthCheckModuleLoader.HealthCheckLoadedModule> LoadedModules { get; set; }
            = new List<HealthCheckModuleLoader.HealthCheckLoadedModule>();
        private List<RegisteredModuleData> RegisteredModules { get; set; } = new List<RegisteredModuleData>();
        private class RegisteredModuleData
        {
            public IHealthCheckModule Module { get; set; }
            public string NameOverride { get; set; }
        }

        private List<ModuleAccessData> RoleModuleAccessLevels { get; set; } = new List<ModuleAccessData>();
        private class ModuleAccessData
        {
            public TAccessRole Roles { get; set; }
            public Type AccessOptionsType { get; set; }
            public object AccessOptions { get; set; }
            public bool FullAccess { get; set; }

            public List<object> GetAllSelectedAccessOptions()
            {
                if (FullAccess)
                {
                    return Enum.GetValues(AccessOptionsType)
                        .OfType<object>()
                        .Where(x => (int)x != 0)
                        .ToList();
                }
                else if (AccessOptions == null)
                {
                    return new List<object>();
                }
                else
                {
                    return EnumUtils.GetFlaggedEnumValues(AccessOptions);
                }
            }
        }

        private List<HealthCheckModuleLoader.HealthCheckLoadedModule> GetModulesRequestHasAccessTo(Maybe<TAccessRole> accessRoles)
            => LoadedModules.Where(x => RequestCanViewModule(accessRoles, x)).ToList();

        private bool RequestCanViewModule(Maybe<TAccessRole> accessRoles, HealthCheckModuleLoader.HealthCheckLoadedModule module)
        {
            if (accessRoles == null)
            {
                return false;
            }

            return RoleModuleAccessLevels.Any(x => 
                x.AccessOptionsType == module.AccessOptionsType
                && EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles));
        }

        private bool RequestHasAccessToModuleMethod(Maybe<TAccessRole> accessRoles,
            HealthCheckModuleLoader.HealthCheckLoadedModule module, HealthCheckModuleLoader.InvokableMethod method)
        {
            var requiredAccessOptions = method.RequiresAccessTo;
            if (requiredAccessOptions == null)
            {
                return RequestCanViewModule(accessRoles, module);
            }

            var accessOptionsType = requiredAccessOptions.GetType();

            return EnumUtils.GetFlaggedEnumValues(requiredAccessOptions).All(opt =>
                RoleModuleAccessLevels.Any(x =>
                    x.AccessOptionsType == accessOptionsType
                    && EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles)
                    && x.GetAllSelectedAccessOptions().Contains(opt))
            );
        }

        private object GetCurrentRequestModuleAccessOptions(
            Maybe<TAccessRole> currentRequestAccessRoles, Type moduleType)
        {
            try
            {
                var accessOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(moduleType);
                var entriesForCurrentRole = RoleModuleAccessLevels
                    .Where(x => EnumUtils.EnumFlagHasAnyFlagsSet(currentRequestAccessRoles.Value, x.Roles)
                                && x.AccessOptionsType == accessOptionsType);
                var values = entriesForCurrentRole.SelectMany(x => x.GetAllSelectedAccessOptions()).ToList();
                var joinedValue = 0;
                foreach(var value in values)
                {
                    joinedValue |= (int)value;
                }
                return Enum.ToObject(accessOptionsType, joinedValue);
            }
            catch (Exception)
            {
                throw new Exception($"Invalid module '{moduleType?.Name}' registered.");
            }
        }

        internal async Task<InvokeModuleMethodResult> InvokeModuleMethod(RequestInformation<TAccessRole> requestInfo, string moduleId, string methodName, string jsonPayload)
        {
            var accessRoles = requestInfo.AccessRole;

            var module = GetModulesRequestHasAccessTo(accessRoles).FirstOrDefault(x => x.Id == moduleId);
            if (module == null)
            {
                return new InvokeModuleMethodResult();
            }

            var method = module.InvokableMethods.FirstOrDefault(x => x.Name == methodName);
            if (method == null || !RequestHasAccessToModuleMethod(accessRoles, module, method))
            {
                return new InvokeModuleMethodResult();
            }

            var context = new HealthCheckModuleContext()
            {
                UserId = requestInfo.UserId,
                UserName = requestInfo.UserName,
                ModuleName = module.Name,

                CurrentRequestRoles = accessRoles.Value,
                CurrentRequestModuleAccessOptions = GetCurrentRequestModuleAccessOptions(accessRoles, module?.Module?.GetType())
            };

            try
            {
                var result = await method.Invoke(module.Module, context, jsonPayload, new NewtonsoftJsonSerializer());
                return new InvokeModuleMethodResult()
                {
                    HasAccess = true,
                    Result = result
                };
            }
            catch(Exception ex)
            {
                return new InvokeModuleMethodResult()
                {
                    HasAccess = true,
                    Result = $"Exception: {ex.Message}"
                };
            }
            finally
            {
                if (context?.AuditEvents != null && context.AuditEvents.Count > 0)
                {
                    foreach (var e in context.AuditEvents)
                    {
                        await Services.AuditEventService.StoreEvent(e);
                    }
                }
            }
        }

        internal class InvokeModuleMethodResult
        {
            public string Result { get; set; }
            public bool HasAccess { get; set; }
        }

        private object GetModuleFrontendOptions(Maybe<TAccessRole> accessRoles)
        {
            var availableModules = GetModulesRequestHasAccessTo(accessRoles);
            var options = new Dictionary<string, object>();
            foreach (var module in availableModules)
            {
                var accessOptions = RoleModuleAccessLevels
                    .Where(x => x.AccessOptionsType == module.AccessOptionsType
                                && EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles))
                    .SelectMany(x => x.GetAllSelectedAccessOptions())
                    .Distinct()
                    .Select(x => x?.ToString())
                    .ToArray();
                options[module.Id] = new
                {
                    Options = module.FrontendOptions,
                    AccessOptions = accessOptions
                };
            }
            return options;
        }

        private List<HealthCheckModuleConfigWrapper> GetModuleConfigs(Maybe<TAccessRole> accessRoles)
        {
            var availableModules = GetModulesRequestHasAccessTo(accessRoles);
            var configs = new List<HealthCheckModuleConfigWrapper>();
            foreach (var module in availableModules)
            {
                configs.Add(new HealthCheckModuleConfigWrapper {
                    Id = module.Id,
                    Name = module.Name,
                    Config = module.Config
                });
            }
            return configs;
        }

        private class HealthCheckModuleConfigWrapper
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public IHealthCheckModuleConfig Config { get; set; }
        }

        /// <summary>
        /// Register a module that will be available.
        /// <para>Optionally override name or id of module.</para>
        /// </summary>
        public TModule UseModule<TModule>(TModule module, string name = null)
            where TModule : IHealthCheckModule
        {
            RegisteredModules.Add(new RegisteredModuleData
            {
                Module = module,
                NameOverride = name
            });
            return module;
        }

        /// <summary>
        /// Grants the given roles access to a module.
        /// <para>ConfigureModuleAccess(MyAccessRoles.Member, ModuleAccess.ViewThing </para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Admin, ModuleAccess.EditThing | ModuleAccess.CreateThing)</para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Guest | MyAccessRoles.Member, ModuleAccess.AnotherThing)</para>
        /// </summary>
        public void GiveRolesAccessToModule<TModuleAccessOptionsEnum>(TAccessRole roles, TModuleAccessOptionsEnum access)
            where TModuleAccessOptionsEnum : Enum
            => RoleModuleAccessLevels.Add(new ModuleAccessData { Roles = roles, AccessOptions = access, AccessOptionsType = typeof(TModuleAccessOptionsEnum) });

        /// <summary>
        /// Grants the given roles access to a module without any specific access options.
        /// </summary>
        public void GiveRolesAccessToModule<TModule>(TAccessRole roles)
            where TModule : IHealthCheckModule
            => RoleModuleAccessLevels.Add(new ModuleAccessData {
                Roles = roles,
                AccessOptions = null,
                AccessOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(typeof(TModule))
            });

        /// <summary>
        /// Grants the given roles access to a module with full access.
        /// </summary>
        public void GiveRolesAccessToModuleWithFullAccess<TModule>(TAccessRole roles)
            where TModule : IHealthCheckModule
            => RoleModuleAccessLevels.Add(new ModuleAccessData {
                Roles = roles,
                AccessOptions = null,
                FullAccess = true,
                AccessOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(typeof(TModule))
            });
        #endregion

#pragma warning disable IDE0060 // Remove unused parameter
        internal void BeforeConfigure(RequestInformation<TAccessRole> currentRequestInformation) { }
#pragma warning restore IDE0060 // Remove unused parameter

        internal void AfterConfigure(RequestInformation<TAccessRole> currentRequestInformation)
        {
            var loader = new HealthCheckModuleLoader();
            LoadedModules = RegisteredModules
                .Select(x => loader.Load(x.Module, GetCurrentRequestModuleAccessOptions(currentRequestInformation.AccessRole, x?.Module?.GetType()), x.NameOverride))
                .Where(x => x != null)
                .ToList();

            foreach (var loadedModule in LoadedModules)
            {
                if (loadedModule.Module is HCTestsModule testsModule)
                {
                    InitStringConverter(testsModule.ParameterConverter);
                }
                else if (loadedModule.Module is HCAuditLogModule auditModule)
                {
                    Services.AuditEventService = auditModule.AuditEventService;
                }
            }
        }

        /// <summary>
        /// Serializes the given object into a json string.
        /// </summary>
        public string SerializeJson(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(obj, settings);
        }

        public bool CanShowPageTo(HealthCheckPageType requestLog, Maybe<TAccessRole> accessRoles) => true;

        private const string Q = "\"";

        /// <summary>
        /// Create view html from the given options.
        /// </summary>
        /// <exception cref="ConfigValidationException"></exception>
        public string CreateViewHtml(Maybe<TAccessRole> accessRoles,
            FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions)
        {
            ValidateConfig(frontEndOptions, pageOptions);

            var moduleConfigs = GetModuleConfigs(accessRoles);
            var moduleConfigData = moduleConfigs.Select(x => new {
                x.Id,
                x.Name,
                x.Config.ComponentName,
                InitialRoute = string.Format(x.Config.InitialRoute, x.Config.DefaultRootRouteSegment),
                RoutePath = string.Format(x.Config.RoutePath, x.Config.DefaultRootRouteSegment)
            });
            var moduleStyleAssets = moduleConfigs.Select(x => x.Config)
                .Where(x => x.LinkTags != null).SelectMany(x => x.LinkTags.Select(t => t.ToString()));
            var moduleScriptAssets = moduleConfigs.Select(x => x.Config)
                .Where(x => x.ScriptTags != null).SelectMany(x => x.ScriptTags.Select(t => t.ToString()));
            var moduleStyleAssetsHtml = string.Join("\n", moduleStyleAssets);
            var moduleScriptAssetsHtml = string.Join("\n", moduleScriptAssets);

            var moduleOptions = GetModuleFrontendOptions(accessRoles);

            var javascriptUrlTags = pageOptions.JavaScriptUrls
                .Select(url => $"<script src=\"{url}\"></script>")
                .ToList();
            var javascriptUrlTagsHtml = string.Join("\n    ", javascriptUrlTags);

            var defaultAssets = !pageOptions.IncludeDefaultAssetLinks ? "" : $@"
    <link href={Q}https://cdn.jsdelivr.net/npm/vuetify@1.5.6/dist/vuetify.min.css{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Montserrat{Q} rel={Q}stylesheet{Q}>
    <link href={Q}https://use.fontawesome.com/releases/v5.7.2/css/all.css{Q} rel={Q}stylesheet{Q} integrity={Q}sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr{Q} crossorigin={Q}anonymous{Q}>";

            var noIndexMeta = pageOptions.IncludeNoIndex ? $"<meta name={Q}robots{Q} content={Q}noindex{Q}>" : "";

            return $@"
<!doctype html>
<html>
<head>
    <title>{pageOptions.PageTitle}</title>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    {moduleStyleAssetsHtml}
    {defaultAssets}
    {pageOptions.CustomHeadHtml}
</head>

<body>
    <div id={Q}app{Q}></div>

    <script>
        window.healthCheckOptions = {JsonConvert.SerializeObject(frontEndOptions) ?? "{}"};
        window.healthCheckModuleOptions = {JsonConvert.SerializeObject(moduleOptions) ?? "{}"};
        window.healthCheckModuleConfigs = {JsonConvert.SerializeObject(moduleConfigData) ?? "{}"};
    </script>
    {moduleScriptAssetsHtml}
    {javascriptUrlTagsHtml}
    {pageOptions.CustomBodyHtml}
</body>
</html>";
        }
        
        /// <summary>
        /// Check if the given roles has access to calling the ping endpoint.
        /// </summary>
        public bool CanUsePingEndpoint(Maybe<TAccessRole> accessRoles)
            => AccessRolesHasAccessTo(accessRoles, AccessOptions.PingAccess, defaultValue: true);

        private void ValidateConfig(FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions)
        {
            frontEndOptions.Validate();
            pageOptions.Validate();
        }

        /// <summary>
        /// Default value if pageAccess is null, false if no roles were given.
        /// </summary>
        private bool AccessRolesHasAccessTo(Maybe<TAccessRole> accessRoles, Maybe<TAccessRole> pageAccess, bool defaultValue = true)
        {
            // No access defined => default
            if (pageAccess == null || !pageAccess.HasValue)
            {
                return defaultValue;
            }
            // Access is defined but no user roles => denied
            else if (accessRoles.HasNothing() && pageAccess.HasValue())
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, pageAccess.Value);
        }

        #region Audit
        /// <summary>
        /// Create a new <see cref="AuditEvent"/> from the given request data and values.
        /// </summary>
        public AuditEvent CreateAuditEventFor(RequestInformation<TAccessRole> request, string area,
            string action, string subject = null)
            => new AuditEvent()
            {
                Area = area,
                Action = action,
                Subject = subject,
                Timestamp = DateTime.Now,
                UserId = request?.UserId,
                UserName = request?.UserName,
                UserAccessRoles = EnumUtils.TryGetEnumFlaggedValueNames(request?.AccessRole.ValueOrNull())
            };
        #endregion

        #region Helpers
        private void InitStringConverter(StringConverter converter)
        {
#if NETFULL
            converter.RegisterConverter<HttpPostedFileBase>(
                (input) => ConvertInputToMemoryFile(input),
                (file) => throw new NotImplementedException());

            converter.RegisterConverter<List<HttpPostedFileBase>>(
                (input) =>
                {
                    var list = new List<HttpPostedFileBase>();
                    if (input == null || string.IsNullOrWhiteSpace(input)) return list;

                    var listItems = JsonConvert.DeserializeObject<List<string>>(input);
                    list.AddRange(
                        listItems
                        .Select(x => ConvertInputToMemoryFile(x))
                        .Where(x => x != null)
                    );

                    return list;
                },
                (file) => throw new NotImplementedException());
#endif
        }

#if NETFULL
        private MemoryFile ConvertInputToMemoryFile(string input)
        {
            if (input == null) return null;

            var parts = input.Split('|');
            if (parts.Length < 3) return null;

            var bytes = Convert.FromBase64String(parts[2]);
            return new MemoryFile(
                contentType: parts[0],
                fileName: parts[1],
                stream: new MemoryStream(bytes)
            );
        }
#endif
        #endregion

        #region Helper classes
        internal class PageType
        {
            public string Id { get; set; }
            public HealthCheckPageType Type { get; set; }
            public Func<HealthCheckControllerHelper<TAccessRole>, Maybe<TAccessRole>, bool> IsVisible { get; set; }
            public Action<FrontEndOptionsViewModel> OnAccessDenied { get; set; }

            public PageType(string id, HealthCheckPageType type,
                Func<HealthCheckControllerHelper<TAccessRole>, Maybe<TAccessRole>, bool> isVisible,
                Action<FrontEndOptionsViewModel> onAccessDenied = null)
            {
                Id = id;
                Type = type;
                IsVisible = isVisible;
                OnAccessDenied = onAccessDenied;
            }
        }
        #endregion
    }
}
