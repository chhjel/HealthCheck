﻿#if NETCORE
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
using HealthCheck.Core.Services.SiteStatus;

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
        /// Must be set for any site statuses to be returned.
        /// </summary>
        protected SiteStatusService SiteStatusService;

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
        /// </summary>
        protected Maybe<TAccessRole> CurrentRequestAccessRoles { get; set; }

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
        /// <para>Returns null by default to allow all test.</para>
        /// </summary>
        protected abstract Maybe<TAccessRole> GetRequestAccessRoles(HttpRequest request);

        /// <summary>
        /// Optionally set config for test set groups. Use the options.SetOptionsFor method to add config for a group by name.
        /// </summary>
        protected virtual void SetTestSetGroupsOptions(TestSetGroupsOptions options) { }

        /// <summary>
        /// Set any options on the test managers here. Method is invoked from BeginExecute.
        /// </summary>
        protected virtual void Configure(HttpRequest request) { }

        /// <summary>
        /// Calls GetRequestAccessRoles and SetOptions.
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext?.Request;
            CurrentRequestAccessRoles = GetRequestAccessRoles(request);
            Configure(request);
            await base.OnActionExecutionAsync(context, next);
        }

        /// <summary>
        /// Returns the page html.
        /// </summary>
        public virtual ActionResult Index()
        {
            if (!Enabled) return NotFound();

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
        /// Get site events to show in the UI.
        /// </summary>
        public virtual async Task<ActionResult> GetSiteEvents()
        {
            if (!Enabled) return NotFound();
            var viewModel = await Helper.GetSiteEventsViewModel(CurrentRequestAccessRoles, SiteStatusService);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        [Route("GetTests")]
        public virtual ActionResult GetTests()
        {
            if (!Enabled) return NotFound();

            SetTestSetGroupsOptions(Helper.TestSetGroupsOptions);
            var viewModel = Helper.GetTestDefinitionsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [HttpPost]
        [Route("ExecuteTest")]
        public virtual async Task<ActionResult> ExecuteTest([FromBody] ExecuteTestInputData data)
        {
            if (!Enabled) return NotFound();

            var result = await Helper.ExecuteTest(CurrentRequestAccessRoles, data);
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
