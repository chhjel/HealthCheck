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

        private readonly HealthCheckControllerHelper<TAccessRole> Helper = new HealthCheckControllerHelper<TAccessRole>();

        /// <summary>
        /// Base controller for the ui and api.
        /// </summary>
        public HealthCheckControllerBase(Assembly assemblyContainingTests)
        {
            Helper.TestDiscoverer.AssemblyContainingTests = assemblyContainingTests ?? throw new ArgumentNullException("An assembly to retrieve tests from must be provided.");
            Helper.GetRequestAccessRolesFunction = () => GetRequestAccessRoles(HttpContext.Request);
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
        /// Set any options on the test managers here. Method is invoked from BeginExecute.
        /// </summary>
        protected virtual void SetOptionalOptions(HttpRequest request, TestRunnerService testRunner, TestDiscoveryService testDiscoverer) { }

        /// <summary>
        /// Should return a custom enum flag object with the roles of the current user. Must match the type used in <see cref="RuntimeTestAttribute.RolesWithAccess"/>.
        /// <para>Returns null by default to allow all test.</para>
        /// </summary>
        protected virtual Maybe<TAccessRole> GetRequestAccessRoles(HttpRequest request) => null;

        /// <summary>
        /// Returns the page html.
        /// </summary>
        public virtual ActionResult Index()
        {
            if (!Enabled) return NotFound();

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(frontEndOptions, pageOptions);

            return new ContentResult()
            {
                Content = html,
                ContentType = "text/html",
            };
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        [Route("GetTests")]
        public virtual ActionResult GetTests()
        {
            if (!Enabled) return NotFound();

            var viewModel = Helper.GetTestDefinitionsViewModel();
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

            var result = await Helper.ExecuteTest(data);
            return CreateJsonResult(result);
        }

        private ActionResult CreateJsonResult(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new StringEnumConverter());

            var json = JsonConvert.SerializeObject(obj, settings);
            return Content(json, "application/json");
        }

        /// <summary>
        /// Calls SetOptionalOptions.
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext?.Request;
            SetOptionalOptions(request, Helper.TestRunner, Helper.TestDiscoverer);
            await base.OnActionExecutionAsync(context, next);
        }
    }
}
#endif
