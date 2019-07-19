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

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for the ui and api.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    public abstract class HealthCheckControllerBase<TAccessRole>: Controller
        where TAccessRole: Enum
    {
        /// <summary>
        /// Must be set for any site statuses to be stored and returned.
        /// </summary>
        protected SiteEventService SiteEventService { get; set; }

        /// <summary>
        /// Must be set for any site audits to be logged.
        /// </summary>
        protected IAuditEventStorage AuditEventService { get; set; }

        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>Enabled by default.</para>
        /// </summary>
        protected bool Enabled { get; set; } = true;

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
        protected Maybe<TAccessRole> CurrentRequestAccessRoles { get; private set; }

        /// <summary>
        /// Information about the current request. Is only set after BeginExecute has been called for the request.
        /// <para>Value equals what you return from GetRequestInformation.</para>
        /// </summary>
        protected RequestInformation<TAccessRole> CurrentRequestInformation { get; set; }

        private readonly HealthCheckControllerHelper<TAccessRole> Helper = new HealthCheckControllerHelper<TAccessRole>();

        /// <summary>
        /// Base controller for the ui and api.
        /// </summary>
        public HealthCheckControllerBase(Assembly assemblyContainingTests)
        {
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
        public virtual ActionResult Index()
        {
            if (!Enabled) return HttpNotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles, SiteEventService, AuditEventService))
            {
                if (!string.IsNullOrWhiteSpace(AccessOptions.RedirectTargetOnNoAccess)) {
                    return Redirect(AccessOptions.RedirectTargetOnNoAccess);
                } else {
                    return HttpNotFound();
                }
            }

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions, SiteEventService, AuditEventService);
            return Content(html);
        }

        /// <summary>
        /// Get filtered audit events to show in the UI.
        /// </summary>
        [HttpPost]
        public virtual async Task<ActionResult> GetFilteredAudits(AuditEventFilterInputData input = null)
        {
            if (!Enabled || !Helper.CanShowAuditPageTo(CurrentRequestAccessRoles, AuditEventService))
                return HttpNotFound();

            var filteredItems = await Helper.GetAuditEventsFilterViewModel(CurrentRequestAccessRoles, input, AuditEventService);
            return CreateJsonResult(filteredItems);
        }

        /// <summary>
        /// Get site events to show in the UI.
        /// </summary>
        public virtual async Task<ActionResult> GetSiteEvents()
        {
            if (!Enabled || !Helper.CanShowOverviewPageTo(CurrentRequestAccessRoles, SiteEventService))
                return HttpNotFound();

            var viewModel = await Helper.GetSiteEventsViewModel(CurrentRequestAccessRoles, SiteEventService);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
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
        [HttpPost]
        public virtual async Task<ActionResult> ExecuteTest(ExecuteTestInputData data)
        {
            if (!Enabled || !Helper.CanShowTestsPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            var result = await Helper.ExecuteTest(CurrentRequestAccessRoles, data);
            Helper.OnTestExecuted(AuditEventService, CurrentRequestInformation, data, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");
    }
}
#endif
