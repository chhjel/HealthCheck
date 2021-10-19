using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.AccessTokens;
using HealthCheck.Core.Modules.AccessTokens.Models;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using HealthCheck.WebUI.Exceptions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NETFULL
using System.IO;
using System.Web;
#endif

#if NETCORE
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
#endif

namespace HealthCheck.WebUI.Util
{
    /// <summary>
    /// Shared code for .net framework/core controllers.
    /// </summary>
    internal class HealthCheckControllerHelper<TAccessRole>
    {
        /// <summary>
        /// Access related options.
        /// </summary>
        public AccessConfig<TAccessRole> AccessConfig { get; set; } = new AccessConfig<TAccessRole>();

        /// <summary>
        /// Used for auditing, is set from first audit module if any.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }

        private readonly Func<HCPageOptions> _pageOptionsGetter;
        private bool _includeClientConnectionDetailsInAllAuditEvents;

        /// <summary>
        /// Initialize a new HealthCheck helper with the given services.
        /// </summary>
        public HealthCheckControllerHelper(Func<HCPageOptions> pageOptionsGetter)
        {
            _pageOptionsGetter = pageOptionsGetter;
            AccessConfig.RoleModuleAccessLevels = RoleModuleAccessLevels;

            TestRunnerService.Serializer = new NewtonsoftJsonSerializer();
            TestRunnerService.GetCurrentTestContext = HealthCheckTestContextHelper.GetCurrentTestContext;
            TestRunnerService.CurrentRequestIsTest = () => HealthCheckTestContextHelper.CurrentRequestIsTest;
            TestRunnerService.SetCurrentRequestIsTest = () => HealthCheckTestContextHelper.CurrentRequestIsTest = true;
        }

        internal bool HasAccessToAnyContent(Maybe<TAccessRole> currentRequestAccessRoles)
            => GetModulesRequestHasAccessTo(currentRequestAccessRoles).Count > 0;

        #region Modules
        private List<HealthCheckLoadedModule> LoadedModules { get; set; } = new List<HealthCheckLoadedModule>();
        private List<RegisteredModuleData> RegisteredModules { get; set; } = new List<RegisteredModuleData>();
        private class RegisteredModuleData
        {
            public IHealthCheckModule Module { get; set; }
            public string NameOverride { get; set; }
        }

        private List<ModuleAccessData<TAccessRole>> RoleModuleAccessLevels { get; set; } = new List<ModuleAccessData<TAccessRole>>();

        internal List<HCFrontEndOptions.HCUserModuleCategories> GetUserModuleCategories(Maybe<TAccessRole> currentRequestAccessRoles)
            => RoleModuleAccessLevels
                .GroupBy(x => x.AccessOptionsType)
                .Select(g =>
                {
                    var accessOptionsType = g.First().AccessOptionsType;
                    var moduleType = GetModuleTypeFromAccessOptionsType(accessOptionsType);
                    var module = LoadedModules.FirstOrDefault(x => x.Type == moduleType);
                    if (module == null || !RequestCanViewModule(currentRequestAccessRoles, module))
                    {
                        return null;
                    }
                    return new HCFrontEndOptions.HCUserModuleCategories
                    {
                        ModuleId = module.Id,
                        ModuleName = module.Name,
                        Categories = g.Any(x => x.FullAccess)
                            ? new List<string>()
                            : (g.SelectMany(x => x.Categories ?? Array.Empty<string>())?.ToList())
                    };
                })
                .Where(x => x != null)
                .OrderBy(x => x.ModuleName)
                .ToList();

        private List<HealthCheckLoadedModule> GetModulesRequestHasAccessTo(Maybe<TAccessRole> accessRoles)
            => LoadedModules.Where(x => RequestCanViewModule(accessRoles, x)).ToList();

        private bool RequestCanViewModule(Maybe<TAccessRole> accessRoles, HealthCheckLoadedModule module)
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
            HealthCheckLoadedModule module, HealthCheckInvokableMethod method)
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

        private bool RequestHasAccessToModuleAction(Maybe<TAccessRole> accessRoles, HealthCheckInvokableAction action)
        {
            var requiredAccessOptions = action.RequiresAccessTo;
            if (requiredAccessOptions == null)
            {
                return true;
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
                throw new HCException($"Invalid module '{moduleType?.Name}' registered.");
            }
        }

        private class HealthCheckInvokableActionWithModule
        {
            public HealthCheckInvokableAction Action { get; set; }
            public HealthCheckLoadedModule Module { get; set; }
        }
        private IEnumerable<HealthCheckInvokableActionWithModule> GetInvokableActionsRequestHasAccessTo(RequestInformation<TAccessRole> requestInfo)
        {
            var accessRoles = requestInfo.AccessRole;
            
            var modules = LoadedModules;
            if (modules == null || !modules.Any())
            {
                return Enumerable.Empty<HealthCheckInvokableActionWithModule>();
            }

            var methods = new List<HealthCheckInvokableActionWithModule>();
            foreach(var module in modules)
            {
                var actions = module.CustomActions;
                foreach(var action in actions)
                {
                    if (RequestHasAccessToModuleAction(accessRoles, action))
                    {
                        methods.Add(new HealthCheckInvokableActionWithModule
                        {
                            Action = action,
                            Module = module
                        });
                    }
                }
            }
            return methods;
        }

        internal async Task<InvokeModuleActionResult> InvokeModuleAction(
            RequestInformation<TAccessRole> requestInfo, string actionName, string url)
        {
            var match = GetInvokableActionsRequestHasAccessTo(requestInfo)
                .FirstOrDefault(x => x.Action.Name?.Trim()?.ToLower() == actionName?.ToLower()?.Trim());
            if (match == null)
            {
                return new InvokeModuleActionResult();
            }

            var accessRoles = requestInfo.AccessRole;
            var action = match.Action;
            var module = match.Module;

            var context = CreateModuleContext(requestInfo, accessRoles, module.Name, module.Id, module.Module);
            try
            {
                var result = await action.Invoke(module.Module, context, url).ConfigureAwait(false);
                return new InvokeModuleActionResult()
                {
                    HasAccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new InvokeModuleActionResult()
                {
                    HasAccess = true,
                    Result = $"Exception: {ex.Message}"
                };
            }
            finally
            {
                if (context?.AuditEvents != null && context.AuditEvents.Count > 0 && AuditEventService != null)
                {
                    foreach (var e in context.AuditEvents.Where(x => x != null))
                    {
                        if (_includeClientConnectionDetailsInAllAuditEvents)
                        {
                            e.AddClientConnectionDetails(context);
                        }

                        await AuditEventService.StoreEvent(e);
                    }
                }
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

            var context = CreateModuleContext(requestInfo, accessRoles, module.Name, module.Id, module.Module);
            try
            {
                var result = await method.Invoke(module.Module, context, jsonPayload, new NewtonsoftJsonSerializer());
                return new InvokeModuleMethodResult()
                {
                    HasAccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new InvokeModuleMethodResult()
                {
                    HasAccess = true,
                    Result = $"Exception: {ex.Message}"
                };
            }
            finally
            {
                if (context?.AuditEvents != null && context.AuditEvents.Count > 0 && AuditEventService != null)
                {
                    foreach (var e in context.AuditEvents.Where(x => x != null))
                    {
                        if (_includeClientConnectionDetailsInAllAuditEvents)
                        {
                            e.AddClientConnectionDetails(context);
                        }

                        await AuditEventService.StoreEvent(e);
                    }
                }
            }
        }

        private HealthCheckModuleContext CreateModuleContext(RequestInformation<TAccessRole> requestInfo, Maybe<TAccessRole> accessRoles,
            string moduleName, string moduleId,
            IHealthCheckModule module)
        {
            var moduleAccess = new List<HealthCheckModuleContext.ModuleAccess>();
            foreach (var access in RoleModuleAccessLevels)
            {
                var accessModuleId = GetModuleTypeFromAccessOptionsType(access.AccessOptionsType)?.Name;
                if (accessModuleId == null) continue;

                var item = moduleAccess.FirstOrDefault(x => x.ModuleId == accessModuleId);
                if (item == null)
                {
                    item = new HealthCheckModuleContext.ModuleAccess
                    {
                        ModuleId = accessModuleId,
                        AccessOptions = new List<string>(),
                        AccessCategories = new List<string>()
                    };
                    moduleAccess.Add(item);
                }

                item.AccessOptions = item.AccessOptions
                    .Union(access.GetAllSelectedAccessOptions().Select(x => x.ToString()))
                    .ToList();
                item.AccessCategories = access.FullAccess
                    ? new List<string>()
                    : access.Categories?.ToList() ?? new List<string>();
            }

            var request = new HealthCheckModuleRequestData()
            {
                Method = requestInfo.Method,
                Headers = requestInfo.Headers,
                RelativeUrl = requestInfo.Url,
                ClientIP = requestInfo.ClientIP
            };

            var pageOptions = _pageOptionsGetter?.Invoke();

            var currentModuleAccess = moduleAccess.FirstOrDefault(x => x.ModuleId == moduleId);
            return new HealthCheckModuleContext()
            {
                UserId = requestInfo.UserId,
                UserName = requestInfo.UserName,
                ModuleName = moduleName,
                ModuleId = moduleId,

                JavaScriptUrls = pageOptions?.JavaScriptUrls ?? new List<string>(),
                CssUrls = pageOptions?.CssUrls ?? new List<string>(),

                CurrentRequestRoles = accessRoles.Value,
                CurrentRequestModuleAccessOptions = GetCurrentRequestModuleAccessOptions(accessRoles, module?.GetType()),

                CurrentRequestModulesAccess = moduleAccess,
                CurrentModuleAccess = currentModuleAccess,
                CurrentModuleCategoryAccess = currentModuleAccess?.AccessCategories ?? new List<string>(),
                LoadedModules = LoadedModules.AsReadOnly(),

                Request = request
            };
        }

        private Type GetModuleTypeFromAccessOptionsType(Type optionsType)
        {
            var baseType = typeof(HealthCheckModuleBase<>).MakeGenericType(optionsType);
            return RegisteredModules.FirstOrDefault(x => baseType.IsInstanceOfType(x.Module))?.Module?.GetType();
        }

        internal class InvokeModuleMethodResult
        {
            public string Result { get; set; }
            public bool HasAccess { get; set; }
        }
        internal class InvokeModuleActionResult
        {
            public bool HasAccess { get; set; }
            public object Result { get; set; }
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
                    Config = module.Config,
                    LoadedSuccessfully = module.LoadedSuccessfully,
                    LoadErrors = module.LoadErrors,
                    LoadErrorStacktrace = module.LoadErrorStacktrace
                });
            }
            return configs;
        }

        private class HealthCheckModuleConfigWrapper
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public IHealthCheckModuleConfig Config { get; set; }
            public bool LoadedSuccessfully { get; set; }
            public List<string> LoadErrors { get; set; }
            public string LoadErrorStacktrace { get; set; }
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
        /// Get the first registered module of the given type.
        /// </summary>
        public TModule GetModule<TModule>() where TModule: class
            => RegisteredModules.FirstOrDefault(x => x.Module is TModule)?.Module as TModule;
        #endregion

        internal bool ApplyTokenAccessIfDetected(RequestInformation<TAccessRole> currentRequestInformation)
        {
            foreach(var module in RegisteredModules)
            {
                if (module.Module is HCAccessTokensModule acModule)
                {
                    var token = acModule.GetTokenForRequest(currentRequestInformation);
                    if (token != null)
                    {
                        ApplyTokenAccess(token, currentRequestInformation);
                        return true;
                    }
                }
            }
            return false;
        }

        private void ApplyTokenAccess(HCAccessToken token, RequestInformation<TAccessRole> currentRequestInformation)
        {
            currentRequestInformation.UserId = token.Id.ToString();
            currentRequestInformation.UserName = $"Token '{token.Name}'";

            var roleValue = 0;
            foreach (var role in token.Roles)
            {
                try
                {
                    var parsedEnumValue = Enum.Parse(typeof(TAccessRole), role);
                    roleValue |= (int)parsedEnumValue;
                }
                catch (Exception) { /* Ignore error here */ }
            }
            currentRequestInformation.AccessRole = new Maybe<TAccessRole>((TAccessRole)Enum.ToObject(typeof(TAccessRole), roleValue));

            foreach (var moduleData in token.Modules)
            {
                var module = RegisteredModules.FirstOrDefault(x => x.Module.GetType().Name == moduleData.ModuleId);
                if (module == null)
                {
                    continue;
                }

                var moduleOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(module.Module.GetType());
                var moduleOptionsValue = 0;
                foreach (var option in moduleData.Options)
                {
                    try
                    {
                        var parsedEnumValue = Enum.Parse(moduleOptionsType, option);
                        moduleOptionsValue |= (int)parsedEnumValue;
                    }
                    catch (Exception) { /* Ignore invalid enum parsing */ }
                }
                var moduleOptions = Enum.ToObject(moduleOptionsType, moduleOptionsValue);

                AccessConfig.GiveRolesAccessToModule(moduleOptionsType, currentRequestInformation.AccessRole.Value, moduleOptions, moduleData.Categories?.ToArray());
            }
        }

        internal void AfterConfigure(RequestInformation<TAccessRole> currentRequestInformation)
        {
            HealthCheckModuleContext createModuleContext(RegisteredModuleData module)
            {
                return CreateModuleContext(currentRequestInformation, currentRequestInformation.AccessRole,
                    module.NameOverride ?? module.Module.GetType().Name, module.Module.GetType().Name, module.Module);
            }

            var loader = new HealthCheckModuleLoader();
            LoadedModules = RegisteredModules
                .Select(x => loader.Load(x.Module, createModuleContext(x), x.NameOverride))
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
                    AuditEventService = auditModule.AuditEventService;
                    _includeClientConnectionDetailsInAllAuditEvents = auditModule.IncludeClientConnectionDetailsInAllEvents;
                }
            }
        }

        /// <summary>
        /// Serializes the given object into a json string.
        /// </summary>
        public string SerializeJson(object obj, bool stringEnums = true)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            if (stringEnums)
            {
                settings.Converters.Add(new StringEnumConverter());
            }

            return JsonConvert.SerializeObject(obj, settings);
        }

        private const string Q = "\"";

        /// <summary>
        /// Create view html from the given options.
        /// </summary>
        /// <exception cref="ConfigValidationException"></exception>
        public string CreateViewHtml(Maybe<TAccessRole> accessRoles,
            HCFrontEndOptions frontEndOptions, HCPageOptions pageOptions)
        {
            ValidateConfig(frontEndOptions, pageOptions);

            var allowStacktrace = AccessRolesHasAccessTo(accessRoles, AccessConfig.ShowFailedModuleLoadStackTrace, defaultValue: false);
            var moduleConfigs = GetModuleConfigs(accessRoles);
            var moduleConfigData = moduleConfigs.Select(x => new {
                x.Id,
                x.Name,
                x.LoadedSuccessfully,
                x.LoadErrors,
                LoadErrorStacktrace = (allowStacktrace ? x.LoadErrorStacktrace : null),
                x.Config?.ComponentName,
                x.Config?.RawHtml,
                InitialRoute = x?.Config?.InitialRoute == null ? null : string.Format(x.Config.InitialRoute, x.Config.DefaultRootRouteSegment),
                RoutePath = x?.Config?.RoutePath == null ? null : string.Format(x.Config.RoutePath, x.Config.DefaultRootRouteSegment)
            });
            var moduleStyleAssets = moduleConfigs.Select(x => x.Config)
                .Where(x => x?.LinkTags != null).SelectMany(x => x.LinkTags.Select(t => t.ToString()))
                .Concat(pageOptions.CssUrls.Select(x => $"<link rel=\"stylesheet\" href=\"{x}\" crossorigin=\"anonymous\" />"));
            var moduleScriptAssets = moduleConfigs.Select(x => x.Config)
                .Where(x => x?.ScriptTags != null).SelectMany(x => x.ScriptTags.Select(t => t.ToString()));
            var styleAssetsHtml = string.Join("\n", moduleStyleAssets);
            var scriptAssetsHtml = string.Join("\n", moduleScriptAssets);

            var moduleOptions = GetModuleFrontendOptions(accessRoles);
            var serializer = new NewtonsoftJsonSerializer();

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
    {styleAssetsHtml}
    {defaultAssets}
    {pageOptions.CustomHeadHtml}
</head>

<body>
    <div id={Q}app{Q}></div>

    <script>
        window.healthCheckOptions = {serializer.Serialize(frontEndOptions) ?? "{}"};
        window.healthCheckModuleOptions = {serializer.Serialize(moduleOptions) ?? "{}"};
        window.healthCheckModuleConfigs = {serializer.Serialize(moduleConfigData) ?? "{}"};
    </script>
    {scriptAssetsHtml}
    {javascriptUrlTagsHtml}
    {pageOptions.CustomBodyHtml}
</body>
</html>";
        }
        private void ValidateConfig(HCFrontEndOptions frontEndOptions, HCPageOptions pageOptions)
        {
            frontEndOptions.Validate();
            pageOptions.Validate();
        }

        #region Access
        /// <summary>
        /// Check if the given roles has access to calling the ping endpoint.
        /// </summary>
        public bool CanUsePingEndpoint(Maybe<TAccessRole> accessRoles)
            => AccessRolesHasAccessTo(accessRoles, AccessConfig.PingAccess, defaultValue: true);

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
#endregion

#region Init module extras
        private void InitStringConverter(StringConverter converter)
        {
            converter.RegisterConverter<byte[]>(
                (input) => ConvertFileInputToBytes(input),
                (file) => null);

            converter.RegisterConverter<List<byte[]>>(
                (input) =>
                {
                    var list = new List<byte[]>();
                    if (input == null || string.IsNullOrWhiteSpace(input)) return list;

                    var listItems = JsonConvert.DeserializeObject<List<string>>(input);
                    list.AddRange(
                        listItems
                        .Select(x => ConvertFileInputToBytes(x))
                        .Where(x => x != null)
                    );

                    return list;
                },
                (file) => null);

#if NETCORE
            converter.RegisterConverter<IFormFile>(
                (input) => ConvertFileInputToMemoryFile(input),
                (file) => null);

            converter.RegisterConverter<List<IFormFile>>(
                (input) =>
                {
                    var list = new List<IFormFile>();
                    if (input == null || string.IsNullOrWhiteSpace(input)) return list;

                    var listItems = JsonConvert.DeserializeObject<List<string>>(input);
                    list.AddRange(
                        listItems
                        .Select(x => ConvertFileInputToMemoryFile(x))
                        .Where(x => x != null)
                    );

                    return list;
                },
                (file) => null);
#endif
#if NETFULL
            converter.RegisterConverter<HttpPostedFileBase>(
                (input) => ConvertFileInputToMemoryFile(input),
                (file) => null);

            converter.RegisterConverter<List<HttpPostedFileBase>>(
                (input) =>
                {
                    var list = new List<HttpPostedFileBase>();
                    if (input == null || string.IsNullOrWhiteSpace(input)) return list;

                    var listItems = JsonConvert.DeserializeObject<List<string>>(input);
                    list.AddRange(
                        listItems
                        .Select(x => ConvertFileInputToMemoryFile(x))
                        .Where(x => x != null)
                    );

                    return list;
                },
                (file) => null);
#endif
        }

        private byte[] ConvertFileInputToBytes(string input)
        {
            if (input == null)
            {
#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
                return null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null
            }

            var parts = input.Split('|');
            if (parts.Length < 3) return null;

            var bytes = Convert.FromBase64String(parts[2]);
            return bytes;
        }

#if NETCORE
        private IFormFile ConvertFileInputToMemoryFile(string input)
        {
            if (input == null) return null;

            var parts = input.Split('|');
            if (parts.Length < 3) return null;

            var bytes = Convert.FromBase64String(parts[2]);

            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", parts[1]);
        }
#endif

#if NETFULL
        private MemoryFile ConvertFileInputToMemoryFile(string input)
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
    }
}
