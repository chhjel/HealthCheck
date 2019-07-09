﻿using HealthCheck.Core.Entities;
using HealthCheck.Core.Services;
using HealthCheck.Core.Services.SiteStatus;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Exceptions;
using HealthCheck.WebUI.Factories;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        public TestRunnerService TestRunner = new TestRunnerService();

        /// <summary>
        /// Discovers tests.
        /// </summary>
        public TestDiscoveryService TestDiscoverer = new TestDiscoveryService();

        /// <summary>
        /// Converts string input from the UI into the types of the method parameters.
        /// </summary>
        public readonly StringConverter ParameterConverter = new StringConverter();

        /// <summary>
        /// Factory for test view models.
        /// </summary>
        public readonly TestViewModelsFactory TestsViewModelsFactory = new TestViewModelsFactory();

        /// <summary>
        /// Factory for site event view models.
        /// </summary>
        public readonly SiteEventViewModelsFactory SiteEventViewModelsFactory = new SiteEventViewModelsFactory();

        /// <summary>
        /// Optional config for test set groups.
        /// </summary>
        public TestSetGroupsOptions TestSetGroupsOptions { get; set; } = new TestSetGroupsOptions();

        private const string Q = "\"";

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
            Maybe<TAccessRole> accessRoles, SiteStatusService service,
            DateTime? from = null, DateTime? to = null)
        {
            if (service == null)
            {
                return new List<SiteEventViewModel>();
            }

            from = from ?? DateTime.Now.AddDays(-30);
            to = to ?? DateTime.Now;
            var viewModel = (await service.GetEvents(from.Value, to.Value))
                .Select(x => SiteEventViewModelsFactory.CreateViewModel(x))
                .ToList();
            return viewModel;
        }

        /// <summary>
        /// Get viewmodel for test sets data.
        /// </summary>
        public TestsDataViewModel GetTestDefinitionsViewModel(Maybe<TAccessRole> accessRoles)
        {
            var model = new TestsDataViewModel()
            {
                TestSets = TestsViewModelsFactory.CreateViewModel(GetTestDefinitions(accessRoles)),
                GroupOptions = TestsViewModelsFactory.CreateViewModel(TestSetGroupsOptions),
            };
            return model;
        }

        /// <summary>
        /// Execute the given test and return a result view model.
        /// </summary>
        public async Task<TestResultViewModel> ExecuteTest(Maybe<TAccessRole> accessRoles, ExecuteTestInputData data)
        {
            if (data == null || data.TestId == null)
            {
                return TestResultViewModel.CreateError("No test id was given.");
            }

            var test = GetTest(accessRoles, data.TestId);
            if (test == null)
            {
                return TestResultViewModel.CreateError($"Test with id '{data.TestId}' not found.", data.TestId);
            }

            try
            {
                var parameters = data?.GetParametersWithConvertedTypes(test.Parameters.Select(x => x.ParameterType).ToArray(), ParameterConverter);
                var result = await TestRunner.ExecuteTest(test, parameters);
                return TestsViewModelsFactory.CreateViewModel(result);
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

        private List<TestClassDefinition> GetTestDefinitions(Maybe<TAccessRole> accessRoles)
        {
            var userRolesMaybe = accessRoles ?? new Maybe<TAccessRole>();
            var userRoles = userRolesMaybe.HasValue ? (object)userRolesMaybe.Value : null;
            return TestDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: userRoles);
        }

        private TestDefinition GetTest(Maybe<TAccessRole> accessRoles, string testId)
            => GetTestDefinitions(accessRoles).SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);
    }
}
