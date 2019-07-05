using HealthCheck.Core.Entities;
using HealthCheck.Core.TestManagers;
using HealthCheck.Core.Util;
using HealthCheck.Web.Core.Exceptions;
using HealthCheck.Web.Core.Factories;
using HealthCheck.Web.Core.Models;
using HealthCheck.Web.Core.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Util
{
    /// <summary>
    /// Shared code for .net framework/core controllers.
    /// </summary>
    public class HealthCheckControllerHelper<TAccessRole>
    {
        /// <summary>
        /// Executes tests.
        /// </summary>
        public TestRunner TestRunner = new TestRunner();

        /// <summary>
        /// Discovers tests.
        /// </summary>
        public TestDiscoverer TestDiscoverer = new TestDiscoverer();

        /// <summary>
        /// Converts string input from the UI into the types of the method parameters.
        /// </summary>
        public readonly StringConverter ParameterConverter = new StringConverter();

        /// <summary>
        /// Factory for view models.
        /// </summary>
        public readonly TestViewModelsFactory ViewModelsFactory = new TestViewModelsFactory();

        /// <summary>
        /// Function that checks access role based on the current request.
        /// </summary>
        public Func<Maybe<TAccessRole>> GetRequestAccessRolesFunction { get; set; }

        private const string Q = "\"";

        /// <summary>
        /// Get viewmodel for test sets data.
        /// </summary>
        public List<TestSetViewModel> GetTestDefinitionsViewModel()
            => ViewModelsFactory.CreateViewModel(GetTestDefinitions());

        /// <summary>
        /// Execute the given test and return a result view model.
        /// </summary>
        public async Task<TestResultViewModel> ExecuteTest(ExecuteTestInputData data)
        {
            // ToDo check access | return CreateError no access

            if (data == null || data.TestId == null)
            {
                return TestResultViewModel.CreateError("No test id was given.");
            }

            var test = GetTest(data.TestId);
            if (test == null)
            {
                return TestResultViewModel.CreateError($"Test with id '{data.TestId}' not found.", data.TestId);
            }

            try
            {
                var parameters = data?.GetParametersWithConvertedTypes(test.Parameters.Select(x => x.ParameterType).ToArray(), ParameterConverter);
                var result = await TestRunner.ExecuteTest(test, parameters);
                return ViewModelsFactory.CreateViewModel(result);
            }
            catch (Exception ex)
            {
                var message = $"Exception: {(ex.InnerException ?? ex).Message}";
                return TestResultViewModel.CreateError(message);
            }
        }

        /// <summary>
        /// Create view html from the given options.
        /// </summary>
        /// <exception cref="ConfigValidationException"></exception>
        public string CreateViewHtml(FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions)
        {
            frontEndOptions.Validate();
            pageOptions.Validate();

            var defaultAssets = !pageOptions.IncludeDefaultAssetLinks ? "" : $@"
    <link href={Q}https://cdn.jsdelivr.net/npm/vuetify@1/dist/vuetify.min.css{Q} rel={Q}stylesheet{Q} />
    <link href='https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons' rel={Q}stylesheet{Q} />
    <link href={Q}https://use.fontawesome.com/releases/v5.7.2/css/all.css{Q} rel={Q}stylesheet{Q} integrity={Q}sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr{Q} crossorigin={Q}anonymous{Q}>";

            var noIndexMeta = pageOptions.IncludeNoIndex ? $"<meta name={Q}robots{Q} content={Q}noindex{Q}>" : "";

            return $@"
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
</html>";
        }

        private List<TestClassDefinition> GetTestDefinitions()
        {
            var userRolesMaybe = GetRequestAccessRolesFunction?.Invoke() ?? new Maybe<TAccessRole>();
            var userRoles = userRolesMaybe.HasValue ? (object)userRolesMaybe.Value : null;
            return TestDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: userRoles);
        }

        private TestDefinition GetTest(string testId)
            => GetTestDefinitions().SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);
    }
}
