using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Exceptions;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.AccessTokens;
using QoDL.Toolkit.Core.Modules.AccessTokens.Models;
using QoDL.Toolkit.Core.Modules.AuditLog;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.Tests;
using QoDL.Toolkit.Core.Modules.Tests.Services;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Modules;
using QoDL.Toolkit.WebUI.Exceptions;
using QoDL.Toolkit.WebUI.Models;
using QoDL.Toolkit.WebUI.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

#if NETFULL
using System.IO;
using System.Web;
#endif

#if NETCORE
using System.IO;
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.WebUI.Util;

/// <summary>
/// Shared code for .net framework/core controllers.
/// </summary>
internal class ToolkitControllerHelper<TAccessRole>
{
    /// <summary>
    /// Access related options.
    /// </summary>
    public AccessConfig<TAccessRole> AccessConfig { get; set; } = new AccessConfig<TAccessRole>();

    /// <summary>
    /// Used for auditing, is set from first audit module if any.
    /// </summary>
    public IAuditEventStorage AuditEventService { get; set; }

    private readonly Func<TKPageOptions> _pageOptionsGetter;
    private bool _includeClientConnectionDetailsInAllAuditEvents;

    /// <summary>
    /// Initialize a new Toolkit helper with the given services.
    /// </summary>
    public ToolkitControllerHelper(Func<TKPageOptions> pageOptionsGetter, Func<TKFrontEndOptions> frontendOptionsGetter)
    {
        _pageOptionsGetter = pageOptionsGetter;
        AccessConfig.RoleModuleAccessLevels = RoleModuleAccessLevels;

        TestRunnerService.Serializer = new NewtonsoftJsonSerializer();
        TestRunnerService.GetCurrentTestContext = ToolkitTestContextHelper.GetCurrentTestContext;
        TestRunnerService.CurrentRequestIsTest = () => ToolkitTestContextHelper.CurrentRequestIsTest;
        TestRunnerService.SetCurrentRequestIsTest = () => ToolkitTestContextHelper.CurrentRequestIsTest = true;

        TKAssetGlobalConfig.EndpointBase ??= frontendOptionsGetter?.Invoke()?.EndpointBase;
    }

    internal bool HasAccessToAnyContent(Maybe<TAccessRole> currentRequestAccessRoles)
        => GetModulesRequestHasAccessTo(currentRequestAccessRoles).Count > 0;

    internal static bool ShouldEnableRequestBuffering() => false;

    #region Modules
    private List<ToolkitLoadedModule> LoadedModules { get; set; } = new List<ToolkitLoadedModule>();
    private List<RegisteredModuleData> RegisteredModules { get; set; } = new List<RegisteredModuleData>();
    private class RegisteredModuleData
    {
        public IToolkitModule Module { get; set; }
        public string NameOverride { get; set; }
    }

    private List<ModuleAccessData<TAccessRole>> RoleModuleAccessLevels { get; set; } = new List<ModuleAccessData<TAccessRole>>();

    internal List<TKFrontEndOptions.TKUserModuleCategories> GetUserModuleCategories(Maybe<TAccessRole> currentRequestAccessRoles)
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
                return new TKFrontEndOptions.TKUserModuleCategories
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

    private List<ToolkitLoadedModule> GetModulesRequestHasAccessTo(Maybe<TAccessRole> accessRoles)
        => LoadedModules.Where(x => RequestCanViewModule(accessRoles, x)).ToList();

    private bool RequestCanViewModule(Maybe<TAccessRole> accessRoles, ToolkitLoadedModule module)
    {
        if (accessRoles == null)
        {
            return false;
        }

        return RoleModuleAccessLevels.Any(x => 
            x.AccessOptionsType == module.AccessOptionsType
            && TKEnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles));
    }

    private bool RequestHasAccessToModuleMethod(Maybe<TAccessRole> accessRoles,
        ToolkitLoadedModule module, ToolkitInvokableMethod method)
    {
        var requiredAccessOptions = method.RequiresAccessTo;
        if (requiredAccessOptions == null)
        {
            return RequestCanViewModule(accessRoles, module);
        }

        var accessOptionsType = requiredAccessOptions.GetType();

        return TKEnumUtils.GetFlaggedEnumValues(requiredAccessOptions).All(opt =>
            RoleModuleAccessLevels.Any(x =>
                x.AccessOptionsType == accessOptionsType
                && TKEnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles)
                && x.GetAllSelectedAccessOptions().Contains(opt))
        );
    }

    private bool RequestHasAccessToModuleAction(Maybe<TAccessRole> accessRoles, ToolkitInvokableAction action)
    {
        var requiredAccessOptions = action.RequiresAccessTo;
        if (requiredAccessOptions == null)
        {
            return true;
        }

        var accessOptionsType = requiredAccessOptions.GetType();

        return TKEnumUtils.GetFlaggedEnumValues(requiredAccessOptions).All(opt =>
            RoleModuleAccessLevels.Any(x =>
                x.AccessOptionsType == accessOptionsType
                && TKEnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles)
                && x.GetAllSelectedAccessOptions().Contains(opt))
        );
    }

    private object GetCurrentRequestModuleAccessOptions(
        Maybe<TAccessRole> currentRequestAccessRoles, Type moduleType)
    {
        try
        {
            var accessOptionsType = ToolkitModuleLoader.GetModuleAccessOptionsType(moduleType);
            var entriesForCurrentRole = RoleModuleAccessLevels
                .Where(x => TKEnumUtils.EnumFlagHasAnyFlagsSet(currentRequestAccessRoles.Value, x.Roles)
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
            throw new TKException($"Invalid module '{moduleType?.Name}' registered.");
        }
    }

    private class ToolkitInvokableActionWithModule
    {
        public ToolkitInvokableAction Action { get; set; }
        public ToolkitLoadedModule Module { get; set; }
    }
    private IEnumerable<ToolkitInvokableActionWithModule> GetInvokableActionsRequestHasAccessTo(RequestInformation<TAccessRole> requestInfo)
    {
        var accessRoles = requestInfo.AccessRole;
        
        var modules = LoadedModules;
        if (modules == null || !modules.Any())
        {
            return Enumerable.Empty<ToolkitInvokableActionWithModule>();
        }

        var methods = new List<ToolkitInvokableActionWithModule>();
        foreach(var module in modules)
        {
            var actions = module.CustomActions;
            foreach(var action in actions)
            {
                if (RequestHasAccessToModuleAction(accessRoles, action))
                {
                    methods.Add(new ToolkitInvokableActionWithModule
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

        var sensitiveDataStripper = (RegisteredModules.FirstOrDefault(x => x?.Module is TKAuditLogModule am)?.Module as TKAuditLogModule)?.SensitiveDataStripper;
        var context = CreateModuleContext(requestInfo, accessRoles, module.Name, module.Id, module.Module, sensitiveDataStripper);
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

    internal async Task<InvokeModuleMethodResult> InvokeModuleMethod(RequestInformation<TAccessRole> requestInfo, string moduleId, string methodName, string jsonPayload, bool isB64 = false)
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

        if (isB64 && !string.IsNullOrWhiteSpace(jsonPayload))
        {
            jsonPayload = Encoding.UTF8.GetString(Convert.FromBase64String(jsonPayload));
        }

        var sensitiveDataStripper = (RegisteredModules.FirstOrDefault(x => x?.Module is TKAuditLogModule am)?.Module as TKAuditLogModule)?.SensitiveDataStripper;
        var context = CreateModuleContext(requestInfo, accessRoles, module.Name, module.Id, module.Module, sensitiveDataStripper);
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
                Result = $"Exception: {ex}"
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

    internal string GetAssetContentType(string n)
    {
        if (n?.EndsWith(".js") == true) return "text/javascript";
        else if (n?.EndsWith(".css") == true) return "text/css";
        else if (n?.EndsWith(".woff2") == true) return "font/woff2";
        else return "text/plain";
    }

    private ToolkitModuleContext CreateModuleContext(RequestInformation<TAccessRole> requestInfo, Maybe<TAccessRole> accessRoles,
        string moduleName, string moduleId,
        IToolkitModule module,
        TKAuditLogModuleOptions.StripSensitiveDataDelegate sensitiveDataStripper)
    {
        var moduleAccess = new List<ToolkitModuleContext.ModuleAccess>();
        var visibleModules = GetModulesRequestHasAccessTo(accessRoles);
        var allowedRoleModuleAccessLevels = RoleModuleAccessLevels.Where(x => visibleModules.Any(m => m.AccessOptionsType == x.AccessOptionsType));
        foreach (var access in allowedRoleModuleAccessLevels)
        {
            var accessModuleId = GetModuleTypeFromAccessOptionsType(access.AccessOptionsType)?.Name;
            if (accessModuleId == null) continue;

            var item = moduleAccess.FirstOrDefault(x => x.ModuleId == accessModuleId);
            if (item == null)
            {
                item = new ToolkitModuleContext.ModuleAccess
                {
                    ModuleId = accessModuleId,
                    AccessOptions = new List<string>(),
                    AccessCategories = new List<string>(),
                    AccessIds = new List<string>()
                };
                moduleAccess.Add(item);
            }

            item.AccessOptions = item.AccessOptions
                .Union(access.GetAllSelectedAccessOptions().Select(x => x.ToString()))
                .ToList();
            item.AccessCategories = access.FullAccess
                ? new List<string>()
                : access.Categories?.ToList() ?? new List<string>();
            item.AccessIds = access.FullAccess
                ? new List<string>()
                : access.Ids?.ToList() ?? new List<string>();
        }

        var request = new ToolkitModuleRequestData()
        {
            Method = requestInfo.Method,
            Headers = requestInfo.Headers,
            RelativeUrl = requestInfo.Url,
            ClientIP = requestInfo.ClientIP,
            InputStream = requestInfo.InputStream,
            FormFiles = requestInfo.FormFiles,
        };

        var pageOptions = _pageOptionsGetter?.Invoke();
        var currentModuleAccess = moduleAccess.FirstOrDefault(x => x.ModuleId == moduleId);
        var endpointBase = TKAssetGlobalConfig.EndpointBase ?? "";

        var jsUrls = pageOptions?.JavaScriptUrls ?? new List<string>();
        if (jsUrls.Count == 0) jsUrls = TKAssetGlobalConfig.DefaultJavaScriptUrls ?? new List<string>();
        jsUrls = jsUrls.Select(x => x.Replace("[base]", endpointBase.TrimEnd('/'))).ToList();
        
        var cssUrls = pageOptions?.CssUrls ?? new List<string>();
        if (cssUrls.Count == 0) cssUrls = TKAssetGlobalConfig.DefaultCssUrls ?? new List<string>();
        cssUrls = cssUrls.Select(x => x.Replace("[base]", endpointBase.TrimEnd('/'))).ToList();

        return new ToolkitModuleContext()
        {
            UserId = requestInfo.UserId,
            UserName = requestInfo.UserName,
            ModuleName = moduleName,
            ModuleId = moduleId,
            CurrentTokenId = requestInfo.CurrentTokenId,
            AllowAccessTokenKillswitch = requestInfo.AllowAccessTokenKillswitch,

            JavaScriptUrls = jsUrls,
            CssUrls = cssUrls,

            CurrentRequestRoles = accessRoles.Value,
            CurrentRequestModuleAccessOptions = GetCurrentRequestModuleAccessOptions(accessRoles, module?.GetType()),

            CurrentRequestModulesAccess = moduleAccess,
            CurrentModuleAccess = currentModuleAccess,
            CurrentModuleCategoryAccess = currentModuleAccess?.AccessCategories ?? new List<string>(),
            CurrentModuleIdAccess = currentModuleAccess?.AccessIds ?? new List<string>(),
            LoadedModules = LoadedModules.AsReadOnly(),

            Request = request,
            SensitiveDataStripper = sensitiveDataStripper
        };
    }

    private Type GetModuleTypeFromAccessOptionsType(Type optionsType)
    {
        var baseType = typeof(ToolkitModuleBase<>).MakeGenericType(optionsType);
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
                            && TKEnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, x.Roles))
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

    private List<ToolkitModuleConfigWrapper> GetModuleConfigs(Maybe<TAccessRole> accessRoles)
    {
        var availableModules = GetModulesRequestHasAccessTo(accessRoles);
        var configs = new List<ToolkitModuleConfigWrapper>();
        foreach (var module in availableModules)
        {
            configs.Add(new ToolkitModuleConfigWrapper {
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

    private class ToolkitModuleConfigWrapper
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IToolkitModuleConfig Config { get; set; }
        public bool LoadedSuccessfully { get; set; }
        public List<string> LoadErrors { get; set; }
        public string LoadErrorStacktrace { get; set; }
    }

    /// <summary>
    /// Register a module that will be available.
    /// <para>Optionally override name or id of module.</para>
    /// </summary>
    public TModule UseModule<TModule>(TModule module, string name = null)
        where TModule : IToolkitModule
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
            if (module.Module is TKAccessTokensModule acModule)
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

    private void ApplyTokenAccess(TKAccessToken token, RequestInformation<TAccessRole> currentRequestInformation)
    {
        currentRequestInformation.UserId = token.Id.ToString();
        currentRequestInformation.UserName = $"Token '{token.Name}'";
        currentRequestInformation.CurrentTokenId = token.Id;
        currentRequestInformation.AllowAccessTokenKillswitch = token.AllowKillswitch;

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

            var moduleOptionsType = ToolkitModuleLoader.GetModuleAccessOptionsType(module.Module.GetType());
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

            AccessConfig.GiveRolesAccessToModule(moduleOptionsType, currentRequestInformation.AccessRole.Value, moduleOptions,
                moduleData.Categories?.ToArray(), moduleData.Ids?.ToArray());
        }
    }

    internal void AfterConfigure(RequestInformation<TAccessRole> currentRequestInformation)
    {
        ToolkitModuleContext createModuleContext(RegisteredModuleData module, TKAuditLogModuleOptions.StripSensitiveDataDelegate sensitiveDataStripper)
        {
            return CreateModuleContext(currentRequestInformation, currentRequestInformation.AccessRole,
                module.NameOverride ?? module.Module.GetType().Name, module.Module.GetType().Name, module.Module, sensitiveDataStripper);
        }

        var sensitiveDataStripper = (RegisteredModules.FirstOrDefault(x => x?.Module is TKAuditLogModule am)?.Module as TKAuditLogModule)?.SensitiveDataStripper;

        var loader = new ToolkitModuleLoader();
        LoadedModules = RegisteredModules
            .Select(x => loader.Load(x.Module, createModuleContext(x, sensitiveDataStripper), x.NameOverride))
            .Where(x => x != null)
            .ToList();

        foreach (var loadedModule in LoadedModules)
        {
            if (loadedModule.Module is TKTestsModule testsModule)
            {
                ToolkitControllerHelper<TAccessRole>.InitStringConverter(testsModule.ParameterConverter);
            }
            else if (loadedModule.Module is TKAuditLogModule auditModule)
            {
                AuditEventService = auditModule.AuditEventService;
                _includeClientConnectionDetailsInAllAuditEvents = auditModule.IncludeClientConnectionDetailsInAllEvents;
            }
        }
    }

    /// <summary>
    /// Serializes the given object into a json string.
    /// </summary>
    public static string SerializeJson(object obj, bool stringEnums = true)
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
        TKFrontEndOptions frontEndOptions, TKPageOptions pageOptions)
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

        var cssTagsHtml = TKAssetGlobalConfig.CreateCssTags(frontEndOptions.EndpointBase, pageOptions?.CssUrls);
        var moduleCssUrls = moduleConfigs.Select(x => x.Config)
            .Where(x => x?.LinkTags != null).SelectMany(x => x.LinkTags.Select(t => t.ToString()));
        cssTagsHtml += string.Join("\n", moduleCssUrls);

        var jsTagsHtml = TKAssetGlobalConfig.CreateJavaScriptTags(frontEndOptions.EndpointBase, pageOptions?.JavaScriptUrls);
        var moduleJsUrls = moduleConfigs.Select(x => x.Config)
            .Where(x => x?.ScriptTags != null).SelectMany(x => x.ScriptTags.Select(t => t.ToString()));
        jsTagsHtml += string.Join("\n", moduleJsUrls);

        var moduleOptions = GetModuleFrontendOptions(accessRoles);
        var serializer = new NewtonsoftJsonSerializer();

        var noIndexMeta = pageOptions.IncludeNoIndex ? $"<meta name={Q}robots{Q} content={Q}noindex{Q}>" : "";
        var loaderStyling = CreateLoaderStyling();

        return $@"
<!doctype html>
<html>
<head>
    <title>{pageOptions.PageTitle}</title>
    <meta charset={Q}utf-8{Q}>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    <script>
        window.__tkLoadErrors = [];
        function showErrorsDelayed() {{
            setTimeout(() => {{
                var errors = window.__tkLoadErrors
                    .filter(x => x.type == 'error' && (x.target.localName == 'script' || x.target.localName == 'link'))
                    .map(x => {{
                        var type = x.target.localName == 'script' ? 'code' : 'style';
                        var url = x.target.localName == 'script' ? x.target.src : x.target.href;
                        return `ERR: Failed to load ${{type}} from '${{url}}'`;
                    }});
                if (errors.length == 0) return;
                var errContainer = document.getElementById('tk-load-errors');
                if (errContainer) {{
                    errContainer.innerHTML = errors.join('\n');
                }}
            }}, 2000);
        }}

        window.addEventListener('error', (x) => {{
            window.__tkLoadErrors.push(event);
            showErrorsDelayed();
        }}, true);
    </script>
    {cssTagsHtml}
    {pageOptions.CustomHeadHtml}
    {loaderStyling}
</head>

<body>
    <div id={Q}app{Q}></div>
    <div id={Q}app-loader{Q} class={Q}floating-squares-effect{Q}>
        <div class={Q}app-loader-text{Q}>[LOADING]</div>
        <ul class={Q}bg-bubbles{Q}>
            <li></li><li></li><li></li><li></li><li></li><li></li><li></li><li></li><li></li><li></li>
        </ul>
        <div id={Q}tk-load-errors{Q}></div>
    </div>

    <script>
        window.toolkitOptions = {serializer.Serialize(frontEndOptions) ?? "{}"};
        window.toolkitModuleOptions = {serializer.Serialize(moduleOptions) ?? "{}"};
        window.toolkitModuleConfigs = {serializer.Serialize(moduleConfigData) ?? "{}"};
    </script>
    {jsTagsHtml}
    {pageOptions.CustomBodyHtml}
</body>
</html>";
    }

    private static string CreateLoaderStyling()
    {
        return @"
<style>
#tk-load-errors {
    white-space: pre;
    text-align: center;
    padding: 10px;
    line-height: 18px;
    font-family: monospace;
    font-weight: 800;
    color: #973a3a;
    text-transform: uppercase;
}
.floating-squares-effect {
    width: 100%;
    height: 100%;
    position: fixed;
    background-color: #fff;
    z-index: -10;
}

.floating-squares-effect .bg-bubbles {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	z-index: 1;
}
.floating-squares-effect .bg-bubbles li {
	position: absolute;
	list-style: none;
	display: block;
	width: 40px;
	height: 40px;
	background-color: #ade5ff;
	bottom: -160px;
	-webkit-animation: square 70s infinite;
	animation: square 70s infinite;
	-webkit-transition-timing-function: linear;
	transition-timing-function: linear;
}
.floating-squares-effect .bg-bubbles li:nth-child(1) {
	left: 10%;
}
.floating-squares-effect .bg-bubbles li:nth-child(2) {
	left: 20%;
	width: 80px;
	height: 80px;
	-webkit-animation-delay: 2s;
	animation-delay: 2s;
	-webkit-animation-duration: 57s;
	animation-duration: 57s;
}
.floating-squares-effect .bg-bubbles li:nth-child(3) {
	left: 25%;
	-webkit-animation-delay: 4s;
	animation-delay: 4s;
}
.floating-squares-effect .bg-bubbles li:nth-child(4) {
	left: 40%;
	width: 60px;
	height: 60px;
	-webkit-animation-duration: 62s;
	animation-duration: 62s;
	background-color: #91c9fa;
}
.floating-squares-effect .bg-bubbles li:nth-child(5) {
	left: 70%;
}
.floating-squares-effect .bg-bubbles li:nth-child(6) {
	left: 80%;
	width: 120px;
	height: 120px;
	-webkit-animation-delay: 3s;
	animation-delay: 3s;
	background-color: #75aede;
}
.floating-squares-effect .bg-bubbles li:nth-child(7) {
	left: 32%;
	width: 160px;
	height: 160px;
	-webkit-animation-delay: 7s;
	animation-delay: 7s;
}
.floating-squares-effect .bg-bubbles li:nth-child(8) {
	left: 55%;
	width: 20px;
	height: 20px;
	-webkit-animation-delay: 15s;
	animation-delay: 15s;
	-webkit-animation-duration: 88s;
	animation-duration: 88s;
}
.floating-squares-effect .bg-bubbles li:nth-child(9) {
	left: 25%;
	width: 10px;
	height: 10px;
	-webkit-animation-delay: 2s;
	animation-delay: 2s;
	-webkit-animation-duration: 90s;
	animation-duration: 90s;
	background-color: #91c9fa;
}
.floating-squares-effect .bg-bubbles li:nth-child(10) {
	left: 90%;
	width: 160px;
	height: 160px;
	-webkit-animation-delay: 28s;
	animation-delay: 28s;
}
@-webkit-keyframes square {
	0% {
		-webkit-transform: translateY(0);
		transform: translateY(0);
	}
	100% {
		-webkit-transform: translateY(-1700px) rotate(600deg);
		transform: translateY(-1700px) rotate(600deg);
	}
}
@keyframes square {
	0% {
		-webkit-transform: translateY(0);
		transform: translateY(0);
	}
	100% {
		-webkit-transform: translateY(-1700px) rotate(600deg);
		transform: translateY(-1700px) rotate(600deg);
	}
}
.app-loader-text {
    font-size: 34px;
    font-family: sans-serif;
    color: #516d87;
    text-align: center;
    margin-top: 12%;
    animation: loaderFadeIn 1s;
}
@keyframes loaderFadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}
</style>
";
    }

    private static void ValidateConfig(TKFrontEndOptions frontEndOptions, TKPageOptions pageOptions)
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
    private static bool AccessRolesHasAccessTo(Maybe<TAccessRole> accessRoles, Maybe<TAccessRole> pageAccess, bool defaultValue = true)
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

        return TKEnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, pageAccess.Value);
    }
#endregion

#region Init module extras
    private static void InitStringConverter(TKStringConverter converter)
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

    private static byte[] ConvertFileInputToBytes(string input)
    {
        if (input == null)
        {
            return null;
        }

        var parts = input.Split('|');
        if (parts.Length < 3) return null;

        var bytes = Convert.FromBase64String(parts[2]);
        return bytes;
    }

#if NETCORE
    private static IFormFile ConvertFileInputToMemoryFile(string input)
    {
        if (input == null) return null;

        var parts = input.Split('|');
        if (parts.Length < 3) return null;

        var bytes = Convert.FromBase64String(parts[2]);

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", parts[1]);
    }
#endif

#if NETFULL
    private static MemoryFile ConvertFileInputToMemoryFile(string input)
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
