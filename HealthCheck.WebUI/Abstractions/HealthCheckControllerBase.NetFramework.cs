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
        protected HealthCheckServiceContainer Services { get; } = new HealthCheckServiceContainer();

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
        [ActionsTestLogInfo(hide: true)]
        public virtual ActionResult Index()
        {
            if (!Enabled) return HttpNotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles))
            {
                if (!string.IsNullOrWhiteSpace(AccessOptions.RedirectTargetOnNoAccess)) {
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
        /// Get filtered audit events to show in the UI.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> GetFilteredAudits(AuditEventFilterInputData input = null)
        {
            if (!Enabled || !Helper.CanShowAuditPageTo(CurrentRequestAccessRoles))
                return HttpNotFound();

            var filteredItems = await Helper.GetAuditEventsFilterViewModel(CurrentRequestAccessRoles, input);
            return CreateJsonResult(filteredItems);
        }

        /// <summary>
        /// Get all request log actions.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        public ActionResult GetRequestLog()
        {
            if (!Enabled || !Helper.CanShowRequestLogPageTo(CurrentRequestAccessRoles))
                return HttpNotFound();

            var viewModel = Helper.GetRequestLogActions(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get site events to show in the UI.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        public virtual async Task<ActionResult> GetSiteEvents()
        {
            if (!Enabled || !Helper.CanShowOverviewPageTo(CurrentRequestAccessRoles))
                return HttpNotFound();

            var viewModel = await Helper.GetSiteEventsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        public virtual ActionResult GetTests()
        {
            if (!Enabled || !Helper.CanShowTestsPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            SetTestSetGroupsOptions(Helper.TestSetGroupsOptions);
            var viewModel = Helper.GetTestDefinitionsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> ExecuteTest(ExecuteTestInputData data)
        {
            if (!Enabled || !Helper.CanShowTestsPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            var result = await Helper.ExecuteTest(CurrentRequestAccessRoles, data);
            Helper.AuditLog_TestExecuted(CurrentRequestInformation, data, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Requests cancellation of the given cancellable test.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> CancelTest(string testId)
        {
            if (!Enabled || !Helper.CanShowTestsPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            return CreateJsonResult(Helper.CancelTest(CurrentRequestInformation, testId));
        }

        /// <summary>
        /// Get log entry search results.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> SearchLogs(LogSearchFilter filter)
        {
            if (!Enabled || !Helper.CanShowLogViewerPageTo(CurrentRequestAccessRoles))
                return HttpNotFound();

            var result = await Helper.SearchLogs(CurrentRequestAccessRoles, filter);
            Helper.AuditLog_LogSearch(CurrentRequestInformation, filter, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Cancels the given log search.
        /// </summary>
        [ActionsTestLogInfo(hide: true)]
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
        [ActionsTestLogInfo(hide: true)]
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
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");
    }
}
#endif
