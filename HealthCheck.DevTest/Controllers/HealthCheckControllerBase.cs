using HealthCheck.Core.TestManagers;
using HealthCheck.Core.Util;
using HealthCheck.Core.Attributes;
using HealthCheck.Web.Core.ViewModels;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Web.Routing;
using System.Web;
using HealthCheck.Web.Core.Models;
using HealthCheck.WebUI.Core;

namespace HealthCheck.DevTest.Controllers
{
    /// <summary>
    /// Base controller for the ui.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    public abstract class HealthCheckControllerBase<TAccessRole>: Controller
        where TAccessRole: Enum
    {
        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>Enabled by default.</para>
        /// </summary>
        protected bool Enabled { get; set; } = true;

        private readonly HealthCheckControllerHelper<TAccessRole> Helper = new HealthCheckControllerHelper<TAccessRole>();

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
        protected virtual void SetOptionalOptions(HttpRequestBase request, TestRunner testRunner, TestDiscoverer testDiscoverer) {}

        /// <summary>
        /// Should return a custom enum flag object with the roles of the current user. Must match the type used in <see cref="RuntimeTestAttribute.RolesWithAccess"/>.
        /// <para>Returns null by default to allow all test.</para>
        /// </summary>
        protected virtual Maybe<TAccessRole> GetRequestAccessRoles(HttpRequestBase request) => null;

        /// <summary>
        /// Returns the page html.
        /// </summary>
        public virtual ActionResult Index()
        {
            if (!Enabled) return HttpNotFound();

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(frontEndOptions, pageOptions);
            return Content(html);
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        public virtual ActionResult GetTests()
        {
            if (!Enabled) return HttpNotFound();

            var viewModel = Helper.GetTestDefinitionsViewModel();
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [HttpPost]
        public virtual async Task<ActionResult> ExecuteTest(ExecuteTestInputData data)
        {
            if (!Enabled) return HttpNotFound();

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

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var request = requestContext?.HttpContext?.Request;
            SetOptionalOptions(request, Helper.TestRunner, Helper.TestDiscoverer);
            return base.BeginExecute(requestContext, callback, state);
        }

    }
}