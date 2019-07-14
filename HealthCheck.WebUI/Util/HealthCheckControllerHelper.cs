using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Services;
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

        /// <summary>
        /// Access related options.
        /// </summary>
        public AccessOptions<TAccessRole> AccessOptions { get; set; } = new AccessOptions<TAccessRole>();

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
            Maybe<TAccessRole> accessRoles, SiteEventService service,
            DateTime? from = null, DateTime? to = null)
        {
            if (!CanShowOverviewPageTo(accessRoles, service))
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
            var invalidTests = new List<TestDefinitionValidationResult>();
            if (CanShowPageTo(accessRoles, AccessOptions.InvalidTestsAccess))
            {
                invalidTests = TestDiscoverer.GetInvalidTests();
            }

            TestDiscoverer.GetInvalidTests();
            var model = new TestsDataViewModel()
            {
                TestSets = TestsViewModelsFactory.CreateViewModel(GetTestDefinitions(accessRoles)),
                GroupOptions = TestsViewModelsFactory.CreateViewModel(TestSetGroupsOptions),
                InvalidTests = invalidTests.Select(x => (TestsViewModelsFactory.CreateViewModel(x))).ToList()
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
                return TestResultViewModel.CreateError("No test id was given.", null, "<Unknown>");
            }

            var test = GetTest(accessRoles, data.TestId);
            if (test == null)
            {
                return TestResultViewModel.CreateError($"Test with id '{data.TestId}' not found.", data.TestId, "<Unknown>");
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
                return TestResultViewModel.CreateError(message, test.Id, test.Name);
            }
        }

        /// <summary>
        /// Create view html from the given options.
        /// </summary>
        /// <exception cref="ConfigValidationException"></exception>
        public string CreateViewHtml(Maybe<TAccessRole> accessRoles,
            FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions,
            SiteEventService siteEventService, IAuditEventStorage auditEventService)
        {
            CheckPageOptions(accessRoles, frontEndOptions, pageOptions, siteEventService, auditEventService);

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
    {pageOptions.CustomBodyHtml}
</body>
</html>";
        }

        /// <summary>
        /// Check if the given roles has access to the any of the pages.
        /// </summary>
        public bool HasAccessToAnyContent(
            Maybe<TAccessRole> accessRoles,
            SiteEventService siteEventService,
            IAuditEventStorage auditEventService)
            => CanShowTestsPageTo(accessRoles)
            || CanShowOverviewPageTo(accessRoles, siteEventService)
            || CanShowAuditPageTo(accessRoles, auditEventService);

        /// <summary>
        /// Check if the given roles has access to the overview page.
        /// </summary>
        public bool CanShowOverviewPageTo(Maybe<TAccessRole> accessRoles, SiteEventService siteEventService)
            => siteEventService != null && CanShowPageTo(accessRoles, AccessOptions.OverviewPageAccess);

        /// <summary>
        /// Check if the given roles has access to the tests page.
        /// </summary>
        public bool CanShowTestsPageTo(Maybe<TAccessRole> accessRoles)
            => CanShowPageTo(accessRoles, AccessOptions.TestsPageAccess);

        /// <summary>
        /// Check if the given roles has access to the audit log page.
        /// </summary>
        public bool CanShowAuditPageTo(Maybe<TAccessRole> accessRoles, IAuditEventStorage auditEventService)
            => auditEventService != null && CanShowPageTo(accessRoles, AccessOptions.AuditLogAccess, defaultValue: false);

        private void CheckPageOptions(Maybe<TAccessRole> accessRoles, FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions,
            SiteEventService siteEventService, IAuditEventStorage auditEventService)
        {
            var deniedEndpoint = "0x90";
            if (CanShowOverviewPageTo(accessRoles, siteEventService))
            {
                frontEndOptions.Pages.Add("overview");
            }
            else
            {
                frontEndOptions.GetSiteEventsEndpoint = deniedEndpoint;
            }

            if (CanShowTestsPageTo(accessRoles))
            {
                frontEndOptions.Pages.Add("tests");
            }
            else
            {
                frontEndOptions.ExecuteTestEndpoint = deniedEndpoint;
                frontEndOptions.GetTestsEndpoint = deniedEndpoint;
            }

            if (CanShowAuditPageTo(accessRoles, auditEventService))
            {
                frontEndOptions.Pages.Add("auditlog");
            }
            else
            {
                frontEndOptions.GetFilteredAuditLogEventsEndpoint = deniedEndpoint;
            }
            frontEndOptions.Validate();
            pageOptions.Validate();
        }

        private List<TestClassDefinition> GetTestDefinitions(Maybe<TAccessRole> accessRoles)
        {
            var userRolesMaybe = accessRoles ?? new Maybe<TAccessRole>();
            var userRoles = userRolesMaybe.HasValue ? (object)userRolesMaybe.Value : null;
            return TestDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: userRoles);
        }
        
        private TestDefinition GetTest(Maybe<TAccessRole> accessRoles, string testId)
            => GetTestDefinitions(accessRoles).SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);

        /// <summary>
        /// Default value if pageAccess is null, false if no roles were given.
        /// </summary>
        private bool CanShowPageTo(Maybe<TAccessRole> accessRoles, Maybe<TAccessRole> pageAccess, bool defaultValue = true)
        {
            // No access defined => default
            if (pageAccess == null || !pageAccess.HasValue)
            {
                return defaultValue;
            }
            // Access is defined but no user roles => denied
            else if (accessRoles.HasNothing() && pageAccess.HasValue())
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, pageAccess.Value);
        }

        #region Audit
        /// <summary>
        /// Create a new <see cref="AuditEvent"/> from the given request data and values.
        /// </summary>
        public AuditEvent CreateAuditEventFor(RequestInformation<TAccessRole> request, AuditEventArea area,
            string action, string subject = null)
            => new AuditEvent()
            {
                Area = area,
                Action = action,
                Subject = subject,
                Timestamp = DateTime.Now,
                UserId = request?.UserId,
                UserName = request?.UserName,
                UserAccessRoles = EnumUtils.TryGetEnumFlaggedValueNames(request?.AccessRole.ValueOrNull())
            };

        /// <summary>
        /// When a test has executed this should be called.
        /// </summary>
        public void OnTestExecuted(IAuditEventStorage auditEventService, RequestInformation<TAccessRole> requestInformation, ExecuteTestInputData input, TestResultViewModel result)
        {
            auditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.Tests, action: "Test executed", subject: result?.TestName)
                .AddDetail("Test id", input?.TestId)
                .AddDetail("Parameters", $"[{string.Join(", ", (input?.Parameters ?? new List<string>()))}]")
                .AddDetail("Result", result?.Message)
                .AddDetail("Duration", $"{result?.DurationInMilliseconds}ms")
            );
        }

        /// <summary>
        /// Get viewmodel for audit filter results.
        /// </summary>
        public async Task<IEnumerable<AuditEventViewModel>> GetAuditEventsFilterViewModel(
            Maybe<TAccessRole> accessRoles,
            AuditEventFilterInputData filter,
            IAuditEventStorage auditEventService)
        {
            if (auditEventService == null || !RoleHasAccessToAuditLogs(accessRoles))
                return Enumerable.Empty<AuditEventViewModel>();

            var from = filter?.FromFilter ?? DateTime.MinValue;
            var to = filter?.ToFilter ?? DateTime.MaxValue;
            var events = await auditEventService.GetEvents(from, to);
            return events
                .Where(x => AuditEventMatchesFilter(x, filter))
                .Select(x => TestsViewModelsFactory.CreateViewModel(x));
        }

        private bool AuditEventMatchesFilter(AuditEvent e, AuditEventFilterInputData filter)
        {
            if (filter == null) return true;
            else if (filter.FromFilter != null && e.Timestamp < filter.FromFilter) return false;
            else if (filter.ToFilter != null && e.Timestamp > filter.ToFilter) return false;
            else if (filter.SubjectFilter != null && e.Subject?.ToLower()?.Contains(filter.SubjectFilter?.ToLower()) != true) return false;
            else if (filter.ActionFilter != null && e.Action?.ToLower()?.Contains(filter.ActionFilter?.ToLower()) != true) return false;
            else if (filter.UserIdFilter != null && e.UserId?.ToLower()?.Contains(filter.UserIdFilter?.ToLower()) != true) return false;
            else if (filter.UserNameFilter != null && e.UserName?.ToLower()?.Contains(filter.UserNameFilter?.ToLower()) != true) return false;
            else if (filter.AreaFilter != null && e.Area != filter.AreaFilter) return false;
            else return true;
        }

        private bool RoleHasAccessToAuditLogs(Maybe<TAccessRole> accessRoles)
        {
            // No access defined or user has no roles => denied
            if (AccessOptions?.AuditLogAccess == null || AccessOptions?.AuditLogAccess?.HasValue != true || accessRoles.HasNothing())
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(accessRoles.Value, AccessOptions.AuditLogAccess.Value);
        }
        #endregion
    }
}
