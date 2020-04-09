#if NETCORE
using HealthCheck.Core.Services;
using HealthCheck.Core.Util;
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.ViewModels;
using HealthCheck.WebUI.Util;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Reflection;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.LogViewer.Models;
using System.Collections.Generic;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Modules.EventNotifications;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for the ui and api.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    public abstract class HealthCheckControllerBase<TAccessRole> : Controller
        where TAccessRole : Enum
    {
        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>Enabled by default.</para>
        /// </summary>
        protected bool Enabled { get; set; } = true;

        /// <summary>
        /// Contains services that enables extra functionality.
        /// </summary>
        protected HealthCheckServiceContainer<TAccessRole> Services { get; } = new HealthCheckServiceContainer<TAccessRole>();

        /// <summary>
        /// Service that executes tests.
        /// </summary>
        protected TestRunnerService TestRunner => Helper.TestRunner;

        /// <summary>
        /// Service that discovers tests.
        /// </summary>
        protected TestDiscoveryService TestDiscoverer => Helper.TestDiscoverer;

        /// <summary>
        /// Options for page access etc.
        /// </summary>
        protected AccessOptions<TAccessRole> AccessOptions => Helper.AccessOptions;

        /// <summary>
        /// Access roles for the current request. Is only set after BeginExecute has been called for the request.
        /// <para>Value equals what you return from GetRequestInformation().AccessRole.</para>
        /// </summary>
        protected Maybe<TAccessRole> CurrentRequestAccessRoles { get; set; }

        /// <summary>
        /// Information about the current request. Is only set after BeginExecute has been called for the request.
        /// <para>Value equals what you return from GetRequestInformation.</para>
        /// </summary>
        protected RequestInformation<TAccessRole> CurrentRequestInformation { get; set; }

        private readonly HealthCheckControllerHelper<TAccessRole> Helper;

        /// <summary>
        /// Base controller for the ui and api.
        /// </summary>
        public HealthCheckControllerBase(Assembly assemblyContainingTests)
        {
            Helper = new HealthCheckControllerHelper<TAccessRole>(Services);
            Helper.TestDiscoverer.AssemblyContainingTests = assemblyContainingTests ?? throw new ArgumentNullException("An assembly to retrieve tests from must be provided.");
        }

        /// <summary>
        /// Get front-end options.
        /// </summary>
        protected abstract FrontEndOptionsViewModel GetFrontEndOptions();

        /// <summary>
        /// Get page options.
        /// </summary>
        protected abstract PageOptions GetPageOptions();

        /// <summary>
        /// Should return a custom enum flag object with the roles of the current user. Must match the type used in <see cref="RuntimeTestAttribute.RolesWithAccess"/>.
        /// <para>Returns null by default to allow all test.</para>
        /// </summary>
        protected abstract RequestInformation<TAccessRole> GetRequestInformation(HttpRequest request);

        /// <summary>
        /// Optionally set config for test set groups. Use the options.SetOptionsFor method to add config for a group by name.
        /// </summary>
        protected virtual void SetTestSetGroupsOptions(TestSetGroupsOptions options) { }

        /// <summary>
        /// Set any options on the test managers here. Method is invoked from BeginExecute.
        /// </summary>
        protected virtual void Configure(HttpRequest request) { }

        /// <summary>
        /// Calls GetRequestInformation and SetOptions.
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext?.Request;
            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestAccessRoles = CurrentRequestInformation?.AccessRole;
            Configure(request);
            await base.OnActionExecutionAsync(context, next);
        }

        /// <summary>
        /// Returns the page html.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult Index()
        {
            if (!Enabled) return NotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles))
            {
                var redirectTarget = AccessOptions.RedirectTargetOnNoAccessUsingRequest?.Invoke(Request);
                if (!string.IsNullOrWhiteSpace(redirectTarget)) {
                    return Redirect(redirectTarget);
                } else if (!string.IsNullOrWhiteSpace(AccessOptions.RedirectTargetOnNoAccess)) {
                    return Redirect(AccessOptions.RedirectTargetOnNoAccess);
                } else {
                    return NotFound();
                }
            }

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);

            return new ContentResult()
            {
                Content = html,
                ContentType = "text/html",
            };
        }

        /// <summary>
        /// Returns 'OK' and 200 status code.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("Ping")]
        public virtual ActionResult Ping()
        {
            if (!Enabled || !Helper.CanUsePingEndpoint(CurrentRequestAccessRoles))
                return NotFound();

            return Content("OK");
        }

        /// <summary>
        /// Get filtered audit events to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetFilteredAudits")]
        public virtual async Task<ActionResult> GetFilteredAudits([FromBody] AuditEventFilterInputData input = null)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.AuditLog, CurrentRequestAccessRoles))
                return NotFound();

            var filteredItems = await Helper.GetAuditEventsFilterViewModel(CurrentRequestAccessRoles, input);
            return CreateJsonResult(filteredItems);
        }

        /// <summary>
        /// Get all request log actions.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetRequestLog")]
        public ActionResult GetRequestLog()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.RequestLog, CurrentRequestAccessRoles))
                return NotFound();

            var viewModel = Helper.GetRequestLogActions(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Clears the requestlog.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("ClearRequestLog")]
        [HttpDelete]
        public ActionResult ClearRequestLog([FromQuery] bool includeDefinitions = false)
        {
            if (!Enabled || !Helper.CanClearRequestLog(CurrentRequestAccessRoles))
                return NotFound();

            Helper.ClearRequestLog(CurrentRequestInformation, includeDefinitions);
            return Content("Cleared");
        }

        /// <summary>
        /// Get site events to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetSiteEvents")]
        public virtual async Task<ActionResult> GetSiteEvents()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Overview, CurrentRequestAccessRoles))
                return NotFound();

            var viewModel = await Helper.GetSiteEventsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetTests")]
        public virtual ActionResult GetTests()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles)) return NotFound();

            SetTestSetGroupsOptions(Helper.TestSetGroupsOptions);
            var viewModel = Helper.GetTestDefinitionsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("ExecuteTest")]
        public virtual async Task<ActionResult> ExecuteTest([FromBody] ExecuteTestInputData data)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles)) return NotFound();

            var result = await Helper.ExecuteTest(CurrentRequestAccessRoles, data);
            Helper.AuditLog_TestExecuted(CurrentRequestInformation, data, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Execute tests in the given category.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("ExecuteTests")]
        public virtual async Task<ActionResult> ExecuteTests(ExecuteTestsInputData data)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles) || data == null) return NotFound();

            var result = await Helper.ExecuteTests(CurrentRequestInformation, data.TestCategory);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("CancelTest")]
        public virtual async Task<ActionResult> CancelTest([FromBody] string testId)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles)) return NotFound();

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            return CreateJsonResult(Helper.CancelTest(CurrentRequestInformation, testId));
        }

        /// <summary>
        /// Get log entry search results.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("SearchLogs")]
        public virtual async Task<ActionResult> SearchLogs([FromBody] LogSearchFilter filter)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.LogViewer, CurrentRequestAccessRoles))
                return NotFound();

            var result = await Helper.SearchLogs(CurrentRequestAccessRoles, filter);
            Helper.AuditLog_LogSearch(CurrentRequestInformation, filter, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Cancels the given log search.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("CancelLogSearch")]
        public virtual async Task<ActionResult> CancelLogSearch([FromBody] string searchId)
        {
            var cancelled = Helper.CancelLogSearch(searchId);
            if (cancelled)
            {
                Helper.AuditLog_LogSearchCancel(CurrentRequestInformation, "Cancelled log search");
            }
            return await Task.FromResult(CreateJsonResult(cancelled));
        }

        /// <summary>
        /// Cancels all log searches.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("CancelAllLogSearches")]
        public virtual async Task<int> CancelAllLogSearches()
        {
            var count = Helper.CancelAllLogSearches();
            if (count > 0)
            {
                Helper.AuditLog_LogSearchCancel(CurrentRequestInformation, "Cancelled all log searches", count);
            }
            return await Task.FromResult(count);
        }

        /// <summary>
        /// Get diagrams to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetDiagrams")]
        public virtual ActionResult GetDiagrams()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Documentation, CurrentRequestAccessRoles)) return NotFound();

            var viewModel = Helper.GetDiagramsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetDataflowStreamsMetadata")]
        public virtual ActionResult GetDataflowStreamsMetadata()
        {
            if (!Enabled || !Helper.CanShowDataflowPageTo(CurrentRequestAccessRoles)) return NotFound();

            var viewModel = Helper.GetDataflowStreamsMetadata(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("GetDataflowStreamEntries")]
        public virtual async Task<ActionResult> GetDataflowStreamEntries([FromBody] GetDataflowStreamEntriesFilter filter)
        {
            if (!Enabled || !Helper.CanShowDataflowPageTo(CurrentRequestAccessRoles)) return NotFound();

            var viewModel = await Helper.GetDataflowEntries(filter.StreamId, filter.StreamFilter, CurrentRequestInformation);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [Route("GetSettings")]
        public virtual ActionResult GetSettings()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Settings, CurrentRequestAccessRoles)) return NotFound();

            var viewModel = Helper.GetSettings(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("SetSettings")]
        public virtual ActionResult SetSettings([FromBody] SetSettingsViewModel model)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Settings, CurrentRequestAccessRoles)) return NotFound();

            Helper.SetSettings(CurrentRequestInformation, model);
            return CreateJsonResult(new { Success = true });
        }

        /// <summary>
        /// Get viewmodel for the event notification configs
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpGet]
        [Route("GetEventNotificationConfigs")]
        public virtual ActionResult GetEventNotificationConfigs()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return NotFound();

            var viewModel = Helper.GetEventNotificationConfigs(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Delete the event notification config with the given id.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("DeleteEventNotificationConfig")]
        public virtual ActionResult DeleteEventNotificationConfig(Guid configId)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return NotFound();

            var success = Helper.DeleteEventNotificationConfig(CurrentRequestInformation, configId);
            return CreateJsonResult(new { Success = success });
        }

        /// <summary>
        /// Enable/disable notification config with the given id.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("SetEventNotificationConfigEnabled")]
        public virtual ActionResult SetEventNotificationConfigEnabled(Guid configId, bool enabled)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return NotFound();

            var success = Helper.SetEventNotificationConfigEnabled(CurrentRequestInformation, configId, enabled);
            return CreateJsonResult(new { Success = success });
        }

        /// <summary>
        /// Save an event notification config.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        [Route("SaveEventNotificationConfig")]
        public virtual ActionResult SaveEventNotificationConfig(EventSinkNotificationConfig config)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return NotFound();

            config = Helper.SaveEventNotificationConfig(CurrentRequestInformation, config);
            return CreateJsonResult(config);
        }

        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");

#region Util
        private T GetSessionObject<T>(string key)
        {
            var json = HttpContext.Session.GetString(key);
            if (json == null) return default(T);
            else return JsonConvert.DeserializeObject<T>(json);
        }

        private void SetSessionObject(string key, object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            HttpContext.Session.SetString(key, json);
        }
#endregion
    }
}
#endif
