using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Diagrams.FlowCharts;
using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Modules.LogViewer.Models;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Exceptions;
using HealthCheck.WebUI.Factories;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Serializers;
using HealthCheck.WebUI.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        /// Factory for site event view models.
        /// </summary>
        public readonly SiteEventViewModelsFactory SiteEventViewModelsFactory = new SiteEventViewModelsFactory();

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

            foreach (var loadedModule in LoadedModules.Where(x => x.Module is HCTestsModule))
            {
                var module = loadedModule.Module as HCTestsModule;
                InitStringConverter(module.ParameterConverter);
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

        /// <summary>
        /// Get viewmodel for test sets data.
        /// </summary>
        public async Task<List<SiteEventViewModel>> GetSiteEventsViewModel(
            Maybe<TAccessRole> accessRoles,
            DateTime? from = null, DateTime? to = null)
        {
            if (!CanShowPageTo(HealthCheckPageType.Overview, accessRoles))
            {
                return new List<SiteEventViewModel>();
            }

            var includeDeveloperDetails = AccessRolesHasAccessTo(accessRoles, AccessOptions.SiteEventDeveloperDetailsAccess, false);
            from ??= DateTime.Now.AddDays(-30);
            to ??= DateTime.Now;
            var viewModel = (await Services.SiteEventService.GetEvents(from.Value, to.Value))
                .Select(x => SiteEventViewModelsFactory.CreateViewModel(x, includeDeveloperDetails))
                .ToList();
            return viewModel;
        }

        /// <summary>
        /// Perform a log search.
        /// </summary>
        public async Task<LogSearchResult> SearchLogs(Maybe<TAccessRole> accessRoles, LogSearchFilter filter)
        {
            if (Services.LogSearcherService == null || !CanShowPageTo(HealthCheckPageType.LogViewer, accessRoles))
                return new LogSearchResult();

            CancellationTokenSource cts = new CancellationTokenSource();

            var search = new LogSearchInProgress
            {
                Id = filter.SearchId ?? Guid.NewGuid().ToString(),
                CancellationTokenSource = cts,
                StartedAt = DateTime.Now
            };
            lock (SearchesInProgress)
            {
                SearchesInProgress.Add(search);
            }

            var result = await Services.LogSearcherService.PerformSearchAsync(filter, cts.Token);

            lock (SearchesInProgress)
            {
                SearchesInProgress.Remove(search);

                // Cleanup any old searches
                lock (SearchesInProgress)
                {
                    AbortLogSearches(threshold: DateTime.Now.AddMinutes(-30));
                }
            }
            return result;
        }

        /// <summary>
        /// Cancel a running search.
        /// </summary>
        public bool CancelLogSearch(string searchId)
        {
            lock (SearchesInProgress)
            {
                var search = SearchesInProgress.FirstOrDefault(x => x.Id == searchId);
                if (search == null)
                    return false;

                try
                {
                    search.CancellationTokenSource.Cancel();
                }
                catch (Exception) { }

                SearchesInProgress.Remove(search);
            }

            return true;
        }

        /// <summary>
        /// Get the number of currently executing log searches across all sessions.
        /// </summary>
        public int GetCurrentlyRunningLogSearchCount()
        {
            lock (SearchesInProgress)
            {
                return SearchesInProgress.Count;
            }
        }

        /// <summary>
        /// Cancel all running log searches from all sessions.
        /// </summary>
        public int CancelAllLogSearches() => AbortLogSearches();

        /// <summary>
        /// Get viewmodel for diagrams data.
        /// </summary>
        public DiagramDataViewModel GetDiagramsViewModel(Maybe<TAccessRole> accessRoles)
        {
            if (!Services.IsAnyDocumentationServiceSet || !CanShowPageTo(HealthCheckPageType.Documentation, accessRoles))
                return new DiagramDataViewModel();

            if (DiagramDataViewModelCache != null)
            {
                return DiagramDataViewModelCache;
            }

            DiagramDataViewModelCache = new DiagramDataViewModel()
            {
                SequenceDiagrams = Services.SequenceDiagramService?.Generate() ?? new List<SequenceDiagram>(),
                FlowCharts = Services.FlowChartsService?.Generate() ?? new List<FlowChart>()
            };
            return DiagramDataViewModelCache;
        }

        public bool CanShowPageTo(HealthCheckPageType requestLog, Maybe<TAccessRole> accessRoles) => true;

        private static DiagramDataViewModel DiagramDataViewModelCache { get; set; }
        private const string Q = "\"";

        /// <summary>
        /// Create view html from the given options.
        /// </summary>
        /// <exception cref="ConfigValidationException"></exception>
        public string CreateViewHtml(Maybe<TAccessRole> accessRoles,
            FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions)
        {
            if (frontEndOptions != null)
            {
                frontEndOptions.CurrentlyRunningLogSearchCount = GetCurrentlyRunningLogSearchCount();
            }

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
        /// Check if the given roles has access to view the dataflow page.
        /// </summary>
        public bool CanShowDataflowPageTo(Maybe<TAccessRole> accessRoles, bool checkCount = true)
            => Services.DataflowService != null 
            && AccessRolesHasAccessTo(accessRoles, AccessOptions.DataflowPageAccess, defaultValue: false)
            && (!checkCount || GetDataflowStreamsMetadata(accessRoles).Any());

        /// <summary>
        /// Check if the given roles has access to calling the ping endpoint.
        /// </summary>
        public bool CanUsePingEndpoint(Maybe<TAccessRole> accessRoles)
            => AccessRolesHasAccessTo(accessRoles, AccessOptions.PingAccess, defaultValue: true);

        /// <summary>
        /// Check if the given roles has access to editing event definitions.
        /// </summary>
        public bool CanEditEventDefinitions(Maybe<TAccessRole> accessRoles)
            => Services.EventSink != null 
            && CanShowPageTo(HealthCheckPageType.EventNotifications, accessRoles)
            && AccessRolesHasAccessTo(accessRoles, AccessOptions.EditEventDefinitionsAccess, defaultValue: false);

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

        #region Log search
        private class LogSearchInProgress
        {
            public string Id { get; set; }
            public DateTime StartedAt { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }

        private static List<LogSearchInProgress> SearchesInProgress { get; set; } = new List<LogSearchInProgress>();
        private int AbortLogSearches(DateTime? threshold = null)
        {
            lock (SearchesInProgress)
            {
                var searchesToCleanup = SearchesInProgress
                    .Where(x => threshold == null || x.StartedAt < threshold).ToList();

                foreach (var search in searchesToCleanup)
                {
                    CancelLogSearch(search.Id);
                }

                return searchesToCleanup.Count;
            }
        }
        #endregion

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

        /// <summary>
        /// When data has been fetched from a datastream this should be called.
        /// </summary>
        public void AuditLog_DatastreamFetched(RequestInformation<TAccessRole> requestInformation,
            DataflowStreamMetadata<TAccessRole> stream, DataflowStreamFilter filter)
        {
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, "Dataflow", action: "Dataflow stream fetched", subject: stream?.Name)
                .AddDetail("Stream id", stream?.Id)
                .AddDetail("Filter input", (filter == null) ? null : SerializeJson(filter))
            );
        }

        /// <summary>
        /// When a log search has been started this should be called.
        /// </summary>
        public void AuditLog_LogSearch(RequestInformation<TAccessRole> requestInformation, LogSearchFilter filter, LogSearchResult result)
        {
            if (result.WasCancelled)
                return;

            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, "LogSearch", action: "Searched logs", subject: filter?.Query)
                .AddDetail("Skip", filter?.Skip.ToString() ?? "null")
                .AddDetail("Take", filter?.Take.ToString() ?? "null")
                .AddDetail("Range", $"{filter?.FromDate?.ToString() ?? "min"} -> {filter?.ToDate?.ToString() ?? "max"}")
                .AddDetail("Result count", result?.Items?.Count.ToString() ?? "null")
                .AddDetail("Duration", $"{result?.DurationInMilliseconds}ms")
            );
        }

        /// <summary>
        /// When a log search has been started this should be called.
        /// </summary>
        public void AuditLog_LogSearchCancel(RequestInformation<TAccessRole> requestInformation, string action, int? count = null)
        {
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, "LogSearch", action: action)
                    .AddDetail("Count", count?.ToString(), onlyIfNotNull: true)
            );
        }

        /// <summary>
        /// When an event notification is deleted.
        /// </summary>
        private void AuditLog_EventNotificationConfigDelete(RequestInformation<TAccessRole> requestInformation, Guid configId)
        {
            var config = Services.EventSink?.GetConfigs()?.FirstOrDefault(x => x.Id == configId);
            if (config != null)
            {
                Services.AuditEventService?.StoreEvent(
                    CreateAuditEventFor(requestInformation, "EventNotifications", action: "Deleted event notification config")
                );
            }
        }

        /// <summary>
        /// When an event notification is created/updated.
        /// </summary>
        private void AuditLog_EventNotificationConfigSaved(RequestInformation<TAccessRole> requestInformation, EventSinkNotificationConfig config)
        {
            if (config != null)
            {
                Services.AuditEventService?.StoreEvent(
                    CreateAuditEventFor(requestInformation, "EventNotifications", action: "Saved event notification config")
                );
            }
        }

        /// <summary>
        /// Get viewmodel for audit filter results.
        /// </summary>
        public async Task<IEnumerable<AuditEventViewModel>> GetAuditEventsFilterViewModel(
            Maybe<TAccessRole> accessRoles,
            AuditEventFilterInputData filter)
        {
            if (Services.AuditEventService == null || !RoleHasAccessToAuditLogs(accessRoles))
                return Enumerable.Empty<AuditEventViewModel>();

            var from = filter?.FromFilter ?? DateTime.MinValue;
            var to = filter?.ToFilter ?? DateTime.MaxValue;
            var events = await Services.AuditEventService.GetEvents(from, to);
            return events
                .Where(x => AuditEventMatchesFilter(x, filter))
                .Select(x => new AuditEventViewModel()
                {
                    Timestamp = x.Timestamp,
                    Area = x.Area,
                    Action = x.Action,
                    Subject = x.Subject,
                    Details = x.Details,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    UserAccessRoles = x.UserAccessRoles,
                });
        }

        /// <summary>
        /// Get viewmodel for dataflow entries result.
        /// </summary>
        public async Task<IEnumerable<IDataflowEntry>> GetDataflowEntries(string streamId, DataflowStreamFilter filter,
            RequestInformation<TAccessRole> requestInformation)
        {
            if (Services.DataflowService == null 
                || !GetDataflowStreamsMetadata(requestInformation.AccessRole).Any())
                return Enumerable.Empty<IDataflowEntry>();

            var stream = this.GetDataflowStreamsMetadata(requestInformation.AccessRole)
                .FirstOrDefault(x => x.Id == streamId);
            if (stream != null)
            {
                AuditLog_DatastreamFetched(requestInformation, stream, filter);
            }

            filter ??= new DataflowStreamFilter();
            filter.PropertyFilters ??= new Dictionary<string, string>();
            return await Services.DataflowService.GetEntries(streamId, filter);
        }

        /// <summary>
        /// Get viewmodel for dataflow streams metadata result.
        /// </summary>
        public IEnumerable<DataflowStreamMetadata<TAccessRole>> GetDataflowStreamsMetadata(Maybe<TAccessRole> accessRoles)
        {
            if (Services.DataflowService == null || !CanShowDataflowPageTo(accessRoles, checkCount: false))
                return Enumerable.Empty<DataflowStreamMetadata<TAccessRole>>();

            return Services.DataflowService.GetStreamMetadata()
                .Where(x => AccessRolesHasAccessTo(accessRoles, x.RolesWithAccess, defaultValue: true));
        }

        /// <summary>
        /// Get viewmodel for the event notification configs
        /// </summary>
        public GetEventNotificationConfigsViewModel GetEventNotificationConfigs(Maybe<TAccessRole> accessRoles)
        {
            if (Services.EventSink == null || !CanShowPageTo(HealthCheckPageType.EventNotifications, accessRoles))
                return new GetEventNotificationConfigsViewModel();

            var notifiers = Services.EventSink.GetNotifiers();
            var configs = Services.EventSink.GetConfigs();
            var definitions = Services.EventSink.GetKnownEventDefinitions();
            var placeholders = Services.EventSink.GetPlaceholders();
            return new GetEventNotificationConfigsViewModel()
            {
                Notifiers = notifiers.Select(x => new EventNotifierViewModel(x)),
                Configs = configs,
                KnownEventDefinitions = definitions,
                Placeholders = placeholders
            };
        }

        /// <summary>
        /// Delete the event notification config with the given id.
        /// </summary>
        public bool DeleteEventNotificationConfig(RequestInformation<TAccessRole> requestInformation, Guid configId)
        {
            if (Services.EventSink == null || !CanShowPageTo(HealthCheckPageType.EventNotifications, requestInformation.AccessRole))
                return false;

            AuditLog_EventNotificationConfigDelete(requestInformation, configId);
            Services.EventSink.DeleteConfig(configId);
            return true;
        }

        /// <summary>
        /// Enable/disable notification config with the given id.
        /// </summary>
        public bool SetEventNotificationConfigEnabled(RequestInformation<TAccessRole> requestInformation, Guid configId, bool enabled)
        {
            if (Services.EventSink == null || !CanShowPageTo(HealthCheckPageType.EventNotifications, requestInformation.AccessRole))
                return false;

            var config = Services.EventSink.GetConfigs().FirstOrDefault(x => x.Id == configId);
            if (config == null)
                return false;

            config.Enabled = enabled;
            config.LastChangedBy = requestInformation?.UserName ?? "Anonymous";
            config.LastChangedAt = DateTime.Now;

            config = Services.EventSink.SaveConfig(config);

            AuditLog_EventNotificationConfigSaved(requestInformation, config);
            return true;
        }

        /// <summary>
        /// Save the event notification config with the given id.
        /// </summary>
        public EventSinkNotificationConfig SaveEventNotificationConfig(RequestInformation<TAccessRole> requestInformation, EventSinkNotificationConfig config)
        {
            if (Services.EventSink == null || !CanShowPageTo(HealthCheckPageType.EventNotifications, requestInformation.AccessRole))
                return config;

            config.LastChangedBy = requestInformation?.UserName ?? "Anonymous";
            config.LastChangedAt = DateTime.Now;

            config.LatestResults ??= new List<string>();
            config.PayloadFilters ??= new List<EventSinkNotificationConfigFilter>();
            config.EventIdFilter ??= new EventSinkNotificationConfigFilter();
            config.NotifierConfigs ??= new List<NotifierConfig>();

            config = Services.EventSink.SaveConfig(config);

            AuditLog_EventNotificationConfigSaved(requestInformation, config);
            return config;
        }

        /// <summary>
        /// Delete a single event definition.
        /// </summary>
        public void DeleteEventDefinition(RequestInformation<TAccessRole> requestInformation, string eventId)
        {
            Services.EventSink?.DeleteDefinition(eventId);
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, "EventNotifications", action: "Delete event definition", eventId)
            );
        }

        /// <summary>
        /// Delete all event definitions.
        /// </summary>
        public void DeleteEventDefinitions(RequestInformation<TAccessRole> requestInformation)
        {
            Services.EventSink?.DeleteDefinitions();
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, "EventNotifications", action: "Delete all event definitions")
            );
        }

        private bool AuditEventMatchesFilter(AuditEvent e, AuditEventFilterInputData filter)
        {
            if (filter == null) return true;
            else if (filter.FromFilter != null && e.Timestamp < filter.FromFilter) return false;
            else if (filter.ToFilter != null && e.Timestamp > filter.ToFilter) return false;
            else if (filter.SubjectFilter != null && e.Subject?.ToLower()?.Contains(filter.SubjectFilter?.ToLower()) != true) return false;
            else if (filter.ActionFilter != null && e.Action?.ToLower()?.Contains(filter.ActionFilter?.ToLower()) != true) return false;
            else if (filter.UserIdFilter != null && e.UserId?.ToLower()?.Contains(filter.UserIdFilter?.ToLower()) != true) return false;
            else if (filter.UserNameFilter != null && e.UserName?.ToLower()?.Contains(filter.UserNameFilter?.ToLower()) != true) return false;
            else if (filter.AreaFilter != null && e.Area != filter.AreaFilter) return false;
            else return true;
        }

        private bool RoleHasAccessToAuditLogs(Maybe<TAccessRole> accessRoles)
        {
            // No access defined or user has no roles => denied
            if (AccessOptions?.AuditLogAccess == null || AccessOptions?.AuditLogAccess?.HasValue != true || accessRoles.HasNothing())
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, AccessOptions.AuditLogAccess.Value);
        }
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
