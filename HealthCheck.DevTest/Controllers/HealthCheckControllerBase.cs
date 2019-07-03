using HealthCheck.Core.Entities;
using HealthCheck.Core.TestManagers;
using HealthCheck.Core.Util;
using HealthCheck.Core.Attributes;
using HealthCheck.Web.Core.Factories;
using HealthCheck.Web.Core.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Web.Routing;
using System.Web;
using HealthCheck.Web.Core.Models;

namespace HealthCheck.DevTest.Controllers
{
    /// <summary>
    /// Base controller for the ui.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    public abstract class HealthCheckControllerBase<TAccessRole>: Controller
        where TAccessRole: Enum
    {
        protected TestRunner _testRunner = new TestRunner();
        protected TestDiscoverer _testDiscoverer = new TestDiscoverer();
        protected readonly StringConverter _stringConverter = new StringConverter();

        private Assembly AssemblyContainingTests { get; }
        private readonly TestViewModelsFactory _testViewModelsFactory = new TestViewModelsFactory();
        private const string Q = "\"";

        public HealthCheckControllerBase(Assembly assemblyContainingTests)
        {
            AssemblyContainingTests = assemblyContainingTests;
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
            var frontEndOptions = GetFrontEndOptions();
            frontEndOptions.Validate();

            var pageOptions = GetPageOptions();
            pageOptions.Validate();

            var defaultAssets = !pageOptions.IncludeDefaultAssetLinks ? "" : $@"
    <link href={Q}https://cdn.jsdelivr.net/npm/vuetify@1/dist/vuetify.min.css{Q} rel={Q}stylesheet{Q} />
    <link href='https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons' rel={Q}stylesheet{Q} />
    <link href={Q}https://use.fontawesome.com/releases/v5.7.2/css/all.css{Q} rel={Q}stylesheet{Q} integrity={Q}sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr{Q} crossorigin={Q}anonymous{Q}>";

            var noIndexMeta = pageOptions.IncludeNoIndex ? $"<meta name={Q}robots{Q} content={Q}noindex{Q}>" : "";

            return Content($@"
<!doctype html>
<html>
<head>
    <title>{pageOptions.PageTitle}</title>
    {pageOptions.CustomHeadHtml}
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    {defaultAssets}
</head>

<body>
    <div id={Q}app{Q}></div>

    <script>
        window.healthCheckOptions = {JsonConvert.SerializeObject(frontEndOptions)};
    </script>
    <script src={Q}{pageOptions.JavaScriptUrl}{Q}></script>
</body>
</html>");
        }

        /// <summary>
        /// Get tests to show in the UI.
        /// </summary>
        public virtual ActionResult GetTests()
        {
            var viewModel = _testViewModelsFactory.CreateViewModel(GetTestDefinitions());
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [HttpPost]
        public virtual async Task<ActionResult> ExecuteTest(ExecuteTestInputData data)
        {
            // ToDo check access | return CreateError no access

            if (data == null || data.TestId == null)
            {
                return CreateJsonResult(TestResultViewModel.CreateError("No test id was given."));
            }

            var test = GetTest(data.TestId);
            if (test == null)
            {
                return CreateJsonResult(TestResultViewModel.CreateError($"Test with id '{data.TestId}' not found.", data.TestId));
            }

            try
            {
                var parameters = data?.GetParametersWithConvertedTypes(test.Parameters.Select(x => x.ParameterType).ToArray(), _stringConverter);
                var result = await _testRunner.ExecuteTest(test, parameters);
                var resultVm = _testViewModelsFactory.CreateViewModel(result);
                return CreateJsonResult(resultVm);
            }
            catch (Exception ex)
            {
                var message = $"Exception: {(ex.InnerException ?? ex).Message}";
                return CreateJsonResult(TestResultViewModel.CreateError(message));
            }
        }

        private List<TestClassDefinition> GetTestDefinitions()
        {
            var userRolesMaybe = GetRequestAccessRoles(Request);
            var userRoles = userRolesMaybe.HasValue ? (object)userRolesMaybe.Value : null;
            return _testDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: userRoles);
        }

        private TestDefinition GetTest(string testId)
            => GetTestDefinitions().SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);


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
            _testDiscoverer = new TestDiscoverer()
            {
                AssemblyContainingTests = AssemblyContainingTests
            };

            var request = requestContext?.HttpContext?.Request;
            SetOptionalOptions(request, _testRunner, _testDiscoverer);
            return base.BeginExecute(requestContext, callback, state);
        }

    }
}