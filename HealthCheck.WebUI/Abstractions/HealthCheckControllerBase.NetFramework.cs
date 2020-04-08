#if NETFULL
using HealthCheck.Core.Services;
using HealthCheck.Core.Util;
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Web.Routing;
using System.Web;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI;
using HealthCheck.WebUI.Util;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using System.Collections.Generic;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Modules.LogViewer.Models;
using System.Web.SessionState;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.EventNotifications;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for the ui and api.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    [SessionState(SessionStateBehavior.ReadOnly)]
    public abstract class HealthCheckControllerBase<TAccessRole>: Controller
        where TAccessRole: Enum
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
        /// </summary>
        protected abstract RequestInformation<TAccessRole> GetRequestInformation(HttpRequestBase request);

        /// <summary>
        /// Optionally set config for test set groups. Use the options.SetOptionsFor method to add config for a group by name.
        /// </summary>
        protected virtual void SetTestSetGroupsOptions(TestSetGroupsOptions options) { }

        /// <summary>
        /// Set any options on the test managers here. Method is invoked from BeginExecute.
        /// </summary>
        protected virtual void Configure(HttpRequestBase request) {}

        /// <summary>
        /// Calls GetRequestAccessRoles and SetOptions.
        /// </summary>
        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var request = requestContext?.HttpContext?.Request;
            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestAccessRoles = CurrentRequestInformation?.AccessRole;
            Configure(request);
            return base.BeginExecute(requestContext, callback, state);
        }

        /// <summary>
        /// Returns the page html.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult Index()
        {
            if (!Enabled) return HttpNotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles))
            {
                var redirectTarget = AccessOptions.RedirectTargetOnNoAccessUsingRequest?.Invoke(Request);
                if (!string.IsNullOrWhiteSpace(redirectTarget)) {
                    return Redirect(redirectTarget);
                } else if (!string.IsNullOrWhiteSpace(AccessOptions.RedirectTargetOnNoAccess)) {
                    return Redirect(AccessOptions.RedirectTargetOnNoAccess);
                } else {
                    return HttpNotFound();
                }
            }

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);
            return Content(html);
        }

        /// <summary>
        /// Returns 'OK' and 200 status code.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult Ping()
        {
            if (!Enabled || !Helper.CanUsePingEndpoint(CurrentRequestAccessRoles))
                return HttpNotFound();

            return Content("OK");
        }

        /// <summary>
        /// Get filtered audit events to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> GetFilteredAudits(AuditEventFilterInputData input = null)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.AuditLog, CurrentRequestAccessRoles))
                return HttpNotFound();

            var filteredItems = await Helper.GetAuditEventsFilterViewModel(CurrentRequestAccessRoles, input);
            return CreateJsonResult(filteredItems);
        }

        /// <summary>
        /// Get all request log actions.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public ActionResult GetRequestLog()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.RequestLog, CurrentRequestAccessRoles))
                return HttpNotFound();

            var viewModel = Helper.GetRequestLogActions(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Clears the requestlog.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpDelete]
        public ActionResult ClearRequestLog(bool includeDefinitions = false)
        {
            if (!Enabled || !Helper.CanClearRequestLog(CurrentRequestAccessRoles))
                return HttpNotFound();

            Helper.ClearRequestLog(CurrentRequestInformation, includeDefinitions);
            return Content("Cleared");
        }

        /// <summary>
        /// Get site events to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual async Task<ActionResult> GetSiteEvents()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Overview, CurrentRequestAccessRoles))
                return HttpNotFound();

            var viewModel = await Helper.GetSiteEventsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetTests()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles)) return HttpNotFound();

            SetTestSetGroupsOptions(Helper.TestSetGroupsOptions);
            var viewModel = Helper.GetTestDefinitionsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> ExecuteTest(ExecuteTestInputData data)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles)) return HttpNotFound();

            var result = await Helper.ExecuteTest(CurrentRequestAccessRoles, data);
            Helper.AuditLog_TestExecuted(CurrentRequestInformation, data, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Execute tests in the given category.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> ExecuteTests(ExecuteTestsInputData data)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles) || data == null) return HttpNotFound();

            var result = await Helper.ExecuteTests(CurrentRequestInformation, data.TestCategory);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Requests cancellation of the given cancellable test.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> CancelTest(string testId)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Tests, CurrentRequestAccessRoles)) return HttpNotFound();

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            return CreateJsonResult(Helper.CancelTest(CurrentRequestInformation, testId));
        }

        /// <summary>
        /// Get log entry search results.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> SearchLogs(LogSearchFilter filter)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.LogViewer, CurrentRequestAccessRoles))
                return HttpNotFound();

            var result = await Helper.SearchLogs(CurrentRequestAccessRoles, filter);
            Helper.AuditLog_LogSearch(CurrentRequestInformation, filter, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Cancels the given log search.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> CancelLogSearch(string searchId)
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
        public virtual ActionResult GetDiagrams()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Documentation, CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetDiagramsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetDataflowStreamsMetadata()
        {
            if (!Enabled || !Helper.CanShowDataflowPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetDataflowStreamsMetadata(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> GetDataflowStreamEntries(GetDataflowStreamEntriesFilter filter)
        {
            if (!Enabled || !Helper.CanShowDataflowPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = await Helper.GetDataflowEntries(filter.StreamId, filter.StreamFilter, CurrentRequestInformation);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetSettings()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Settings, CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetSettings(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult SetSettings(SetSettingsViewModel model)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Settings, CurrentRequestAccessRoles)) return HttpNotFound();

            Helper.SetSettings(CurrentRequestInformation, model);
            return CreateJsonResult(new { Success = true });
        }

        /// <summary>
        /// Get viewmodel for the event notification configs
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetEventNotificationConfigs()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetEventNotificationConfigs(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Delete the event notification config with the given id.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult DeleteEventNotificationConfig(Guid configId)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            var success = Helper.DeleteEventNotificationConfig(CurrentRequestInformation, configId);
            return CreateJsonResult(new { Success = success });
        }

        /// <summary>
        /// Enable/disable notification config with the given id.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult SetEventNotificationConfigEnabled(Guid configId, bool enabled)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            var success = Helper.SetEventNotificationConfigEnabled(CurrentRequestInformation, configId, enabled);
            return CreateJsonResult(new { Success = success });
        }

        /// <summary>
        /// Save an event notification config.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult SaveEventNotificationConfig(EventSinkNotificationConfig config)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            config = Helper.SaveEventNotificationConfig(CurrentRequestInformation, config);
            return CreateJsonResult(config);
        }

        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");
    }
}
#endif
