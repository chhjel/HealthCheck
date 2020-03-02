using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.RequestLog.Models;
using HealthCheck.Core.Modules.LogViewer.Models;
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
using System.Threading;
using System.Threading.Tasks;
using HealthCheck.WebUI.Models.Api;
using System.Web;
using System.IO;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using HealthCheck.Core.Modules.Diagrams.FlowCharts;
using HealthCheck.Core.Modules.Settings;

namespace HealthCheck.WebUI.Util
{
    /// <summary>
    /// Shared code for .net framework/core controllers.
    /// </summary>
    public class HealthCheckControllerHelper<TAccessRole>
    {
        /// <summary>
        /// Initialize a new HealthCheck helper with the given services.
        /// </summary>
        public HealthCheckControllerHelper(HealthCheckServiceContainer<TAccessRole> serviceContainer)
        {
            Services = serviceContainer ?? new HealthCheckServiceContainer<TAccessRole>();

#if NETFULL
            ParameterConverter.RegisterConverter<HttpPostedFileBase>(
                (input) => ConvertInputToMemoryFile(input),
                (file) => throw new NotImplementedException());

            ParameterConverter.RegisterConverter<List<HttpPostedFileBase>>(
                (input) =>
                {
                    var list = new List<HttpPostedFileBase>();
                    if (input == null || string.IsNullOrWhiteSpace(input)) return list;

                    var listItems = JsonConvert.DeserializeObject<List<string>>(input);
                    list.AddRange(
                        listItems
                        .Select(x => ConvertInputToMemoryFile(x))
                        .Where(x => x != null)
                    );

                    return list;
                },
                (file) => throw new NotImplementedException());
#endif
        }

#if NETFULL
        private MemoryFile ConvertInputToMemoryFile(string input)
        {
            if (input == null) return null;

            var parts = input.Split('|');
            if (parts.Length < 3) return null;

            var bytes = Convert.FromBase64String(parts[2]);
            return new MemoryFile(
                contentType: parts[0],
                fileName: parts[1],
                stream: new MemoryStream(bytes)
            );
        }
#endif

        /// <summary>
        /// Contains services that enables extra functionality.
        /// </summary>
        public HealthCheckServiceContainer<TAccessRole> Services { get; } = new HealthCheckServiceContainer<TAccessRole>();

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

        private const string deniedEndpoint = "0x90";
        private readonly PageType[] Pages = new[]
        {
            new PageType("status", HealthCheckPageType.Overview,
                isVisible: (c, a) => c.Services.SiteEventService != null && c.AccessRolesHasAccessTo(a, c.AccessOptions.OverviewPageAccess),
                onAccessDenied: (o) => o.GetSiteEventsEndpoint = deniedEndpoint),

            new PageType("tests", HealthCheckPageType.Tests,
                isVisible: (c, a) => c.AccessRolesHasAccessTo(a, c.AccessOptions.TestsPageAccess),
                onAccessDenied: (o) => o.CancelTestEndpoint = o.ExecuteTestEndpoint = o.GetTestsEndpoint = deniedEndpoint),

            new PageType("auditlog", HealthCheckPageType.AuditLog,
                isVisible: (c, a) => c.Services.AuditEventService != null && c.AccessRolesHasAccessTo(a, c.AccessOptions.AuditLogAccess, defaultValue: false),
                onAccessDenied: (o) => o.GetFilteredAuditLogEventsEndpoint = deniedEndpoint),

            new PageType("logviewer", HealthCheckPageType.LogViewer,
                isVisible: (c, a) => c.Services.LogSearcherService != null && c.AccessRolesHasAccessTo(a, c.AccessOptions.LogViewerPageAccess, defaultValue: false),
                onAccessDenied: (o) => o.GetLogSearchResultsEndpoint = o.CancelLogSearchEndpoint = o.CancelAllLogSearchesEndpoint = deniedEndpoint),

            new PageType("requestlog", HealthCheckPageType.RequestLog,
                isVisible: (c, a) => c.Services.RequestLogService != null && c.AccessRolesHasAccessTo(a, c.AccessOptions.RequestLogPageAccess, defaultValue: false),
                onAccessDenied: (o) => o.GetRequestLogEndpoint = deniedEndpoint),

            new PageType("documentation", HealthCheckPageType.Documentation,
                isVisible: (c, a) => c.Services.IsAnyDocumentationServiceSet && c.AccessRolesHasAccessTo(a, c.AccessOptions.DocumentationPageAccess, defaultValue: false),
                onAccessDenied: (o) => o.DiagramsDataEndpoint = deniedEndpoint),

            new PageType("dataflow", HealthCheckPageType.Dataflow,
                isVisible: (c, a) => c.CanShowDataflowPageTo(a),
                onAccessDenied: (o) => o.GetDataflowStreamsMetadataEndpoint = o.GetDataflowStreamEntriesEndpoint = deniedEndpoint),

            new PageType("settings", HealthCheckPageType.Settings,
                isVisible: (c, a) => c.Services.SettingsService != null && c.AccessRolesHasAccessTo(a, c.AccessOptions.SettingsPageAccess, defaultValue: false),
                onAccessDenied: (o) => o.GetSettingsEndpoint = o.SetSettingsEndpoint = deniedEndpoint)
        };

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
        /// Get all request log actions.
        /// </summary>
        public List<LoggedEndpointDefinition> GetRequestLogActions(Maybe<TAccessRole> accessRoles)
        {
            if (!CanShowPageTo(HealthCheckPageType.RequestLog, accessRoles))
            {
                return new List<LoggedEndpointDefinition>();
            }

            return Services?.RequestLogService?.GetRequests() ?? new List<LoggedEndpointDefinition>();
        }

        /// <summary>
        /// Clear the requestlog.
        /// </summary>
        public void ClearRequestLog(RequestInformation<TAccessRole> requestInformation, bool includeDefinitions)
        {
            if (!CanClearRequestLog(requestInformation.AccessRole))
            {
                return;
            }

            var auditAction = (includeDefinitions) ? "Cleared request log + definitions" : "Cleared request log";
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.RequestLog, action: auditAction)
            );
            Services?.RequestLogService?.ClearRequests(includeDefinitions);
        }

        /// <summary>
        /// Get viewmodel for test sets data.
        /// </summary>
        public async Task<List<SiteEventViewModel>> GetSiteEventsViewModel(
            Maybe<TAccessRole> accessRoles,
            DateTime? from = null, DateTime? to = null)
        {
            if (!CanShowPageTo(HealthCheckPageType.Overview, accessRoles))
            {
                return new List<SiteEventViewModel>();
            }

            var includeDeveloperDetails = AccessRolesHasAccessTo(accessRoles, AccessOptions.SiteEventDeveloperDetailsAccess, false);
            from = from ?? DateTime.Now.AddDays(-30);
            to = to ?? DateTime.Now;
            var viewModel = (await Services.SiteEventService.GetEvents(from.Value, to.Value))
                .Select(x => SiteEventViewModelsFactory.CreateViewModel(x, includeDeveloperDetails))
                .ToList();
            return viewModel;
        }

        /// <summary>
        /// Get viewmodel for test sets data.
        /// </summary>
        public TestsDataViewModel GetTestDefinitionsViewModel(Maybe<TAccessRole> accessRoles)
        {
            var invalidTests = new List<TestDefinitionValidationResult>();
            if (AccessRolesHasAccessTo(accessRoles, AccessOptions.InvalidTestsAccess))
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
                var result = await TestRunner.ExecuteTest(test, parameters, allowDefaultValues: false);
                return TestsViewModelsFactory.CreateViewModel(result);
            }
            catch (Exception ex)
            {
                var message = $"Exception: {(ex.InnerException ?? ex).Message}";
                return TestResultViewModel.CreateError(message, test.Id, test.Name);
            }
        }

        /// <summary>
        /// Execute tests in the given category and return a result view model.
        /// </summary>
        public async Task<ExecuteTestsResult> ExecuteTests(RequestInformation<TAccessRole> requestInformation, string testCategory)
        {
            if (testCategory == null)
            {
                return new ExecuteTestsResult { TotalResult = TestResultStatus.Error, ErrorMessage = "No category to test was given." };
            }

            try
            {
                var results = await TestRunner.ExecuteTests(TestDiscoverer,
                    testFilter: (test) => 
                        test.Categories.Contains(testCategory)
                        && EnumUtils.EnumFlagHasAnyFlagsSet(requestInformation.AccessRole.Value, test.RolesWithAccess),
                    auditEventService: Services.AuditEventService,
                    auditUserId: requestInformation?.UserId,
                    auditUsername: requestInformation?.UserName
                );
                var testResults = results.Select(x => new ExecuteTestsTestResult()
                {
                    TestId = x.Test?.Id,
                    TestName = x.Test?.Name,
                    Result = x.Status,
                    Message = x.Message,
                    StackTrace = x.StackTrace
                }).ToList();

                return new ExecuteTestsResult()
                {
                    TotalResult = testResults.Any() 
                        ? testResults.OrderByDescending(x => (int)x.Result).First().Result
                        : TestResultStatus.Success,
                    Results = testResults
                };
            }
            catch (Exception ex)
            {
                var message = $"Exception: {(ex.InnerException ?? ex).Message}";
                return new ExecuteTestsResult { TotalResult = TestResultStatus.Error, ErrorMessage = $"An exception occured during test execution. {message}" };
            }
        }

        /// <summary>
        /// Requests cancellation of the given cancellable test.
        /// </summary>
        public bool CancelTest(RequestInformation<TAccessRole> requestInfo, string testId)
        {
            if (testId == null)
            {
                return false;
            }

            var test = GetTest(requestInfo.AccessRole, testId);
            if (test == null)
            {
                return false;
            }

            var registered = TestRunner.RequestTestCancellation(testId);
            if (registered)
            {
                Services.AuditEventService?.StoreEvent(
                    CreateAuditEventFor(requestInfo, AuditEventArea.Tests, action: "Test cancellation requested", subject: test?.Name)
                    .AddDetail("Test id", test?.Id)
                );
            }
            return registered;
        }

        /// <summary>
        /// Perform a log search.
        /// </summary>
        public async Task<LogSearchResult> SearchLogs(Maybe<TAccessRole> accessRoles, LogSearchFilter filter)
        {
            if (Services.LogSearcherService == null || !CanShowPageTo(HealthCheckPageType.LogViewer, accessRoles))
                return new LogSearchResult();

            CancellationTokenSource cts = new CancellationTokenSource();

            var search = new LogSearchInProgress
            {
                Id = filter.SearchId ?? Guid.NewGuid().ToString(),
                CancellationTokenSource = cts,
                StartedAt = DateTime.Now
            };
            lock (SearchesInProgress)
            {
                SearchesInProgress.Add(search);
            }

            var result = await Services.LogSearcherService.PerformSearchAsync(filter, cts.Token);

            lock (SearchesInProgress)
            {
                SearchesInProgress.Remove(search);

                // Cleanup any old searches
                lock (SearchesInProgress)
                {
                    AbortLogSearches(threshold: DateTime.Now.AddMinutes(-30));
                }
            }
            return result;
        }

        /// <summary>
        /// Cancel a running search.
        /// </summary>
        public bool CancelLogSearch(string searchId)
        {
            lock (SearchesInProgress)
            {
                var search = SearchesInProgress.FirstOrDefault(x => x.Id == searchId);
                if (search == null)
                    return false;

                try
                {
                    search.CancellationTokenSource.Cancel();
                }
                catch (Exception) { }

                SearchesInProgress.Remove(search);
            }

            return true;
        }

        /// <summary>
        /// Get the number of currently executing log searches across all sessions.
        /// </summary>
        public int GetCurrentlyRunningLogSearchCount()
        {
            lock (SearchesInProgress)
            {
                return SearchesInProgress.Count;
            }
        }

        /// <summary>
        /// Cancel all running log searches from all sessions.
        /// </summary>
        public int CancelAllLogSearches() => AbortLogSearches();

        /// <summary>
        /// Get viewmodel for diagrams data.
        /// </summary>
        public DiagramDataViewModel GetDiagramsViewModel(Maybe<TAccessRole> accessRoles)
        {
            if (!Services.IsAnyDocumentationServiceSet || !CanShowPageTo(HealthCheckPageType.Documentation, accessRoles))
                return new DiagramDataViewModel();

            if (DiagramDataViewModelCache != null)
            {
                return DiagramDataViewModelCache;
            }

            DiagramDataViewModelCache = new DiagramDataViewModel()
            {
                SequenceDiagrams = Services.SequenceDiagramService?.Generate() ?? new List<SequenceDiagram>(),
                FlowCharts = Services.FlowChartsService?.Generate() ?? new List<FlowChart>()
            };
            return DiagramDataViewModelCache;
        }
        private static DiagramDataViewModel DiagramDataViewModelCache { get; set; }
        private const string Q = "\"";

        /// <summary>
        /// Create view html from the given options.
        /// </summary>
        /// <exception cref="ConfigValidationException"></exception>
        public string CreateViewHtml(Maybe<TAccessRole> accessRoles,
            FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions)
        {
            if (frontEndOptions != null)
            {
                frontEndOptions.CurrentlyRunningLogSearchCount = GetCurrentlyRunningLogSearchCount();
            }

            CheckPageOptions(accessRoles, frontEndOptions, pageOptions);
            var javascriptUrlTags = pageOptions.JavaScriptUrls
                .Select(url => $"<script src=\"{url}\"></script>")
                .ToList();
            var javascriptUrlTagsHtml = string.Join("\n    ", javascriptUrlTags);

            var defaultAssets = !pageOptions.IncludeDefaultAssetLinks ? "" : $@"
    <link href={Q}https://cdn.jsdelivr.net/npm/vuetify@1.5.6/dist/vuetify.min.css{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons{Q} rel={Q}stylesheet{Q} />
    <link href={Q}https://fonts.googleapis.com/css?family=Montserrat{Q} rel={Q}stylesheet{Q}>
    <link href={Q}https://use.fontawesome.com/releases/v5.7.2/css/all.css{Q} rel={Q}stylesheet{Q} integrity={Q}sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr{Q} crossorigin={Q}anonymous{Q}>";

            var noIndexMeta = pageOptions.IncludeNoIndex ? $"<meta name={Q}robots{Q} content={Q}noindex{Q}>" : "";

            return $@"
<!doctype html>
<html>
<head>
    <title>{pageOptions.PageTitle}</title>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    {defaultAssets}
    {pageOptions.CustomHeadHtml}
</head>

<body>
    <div id={Q}app{Q}></div>

    <script>
        window.healthCheckOptions = {JsonConvert.SerializeObject(frontEndOptions)};
    </script>
    {javascriptUrlTagsHtml}
    {pageOptions.CustomBodyHtml}
</body>
</html>";
        }

        /// <summary>
        /// Check if the given roles has access to the any of the pages.
        /// </summary>
        public bool HasAccessToAnyContent(Maybe<TAccessRole> accessRoles)
            => Pages.Any(x => x.IsVisible(this, accessRoles));

        /// <summary>
        /// Check if the given roles has access to clearing the requestlog.
        /// </summary>
        public bool CanClearRequestLog(Maybe<TAccessRole> accessRoles)
            => Services.RequestLogService != null && AccessRolesHasAccessTo(accessRoles, AccessOptions.ClearRequestLogAccess, defaultValue: false);

        /// <summary>
        /// Check if the given roles has access to view the dataflow page.
        /// </summary>
        public bool CanShowDataflowPageTo(Maybe<TAccessRole> accessRoles, bool checkCount = true)
            => Services.DataflowService != null 
            && AccessRolesHasAccessTo(accessRoles, AccessOptions.DataflowPageAccess, defaultValue: false)
            && (!checkCount || GetDataflowStreamsMetadata(accessRoles).Any());

        /// <summary>
        /// Check if the given roles has access to calling the ping endpoint.
        /// </summary>
        public bool CanUsePingEndpoint(Maybe<TAccessRole> accessRoles)
            => AccessRolesHasAccessTo(accessRoles, AccessOptions.PingAccess, defaultValue: true);

        private void CheckPageOptions(Maybe<TAccessRole> accessRoles, FrontEndOptionsViewModel frontEndOptions, PageOptions pageOptions)
        {
            foreach(var page in Pages)
            {
                if (page.IsVisible(this, accessRoles))
                {
                    frontEndOptions.Pages.Add(page.Id);
                }
                else
                {
                    page.OnAccessDenied(frontEndOptions);
                }
            }

            if (!CanClearRequestLog(accessRoles))
            {
                frontEndOptions.ClearRequestLogEndpoint = deniedEndpoint;
                frontEndOptions.HasAccessToClearRequestLog = false;
            }

            PrioritizePages(frontEndOptions.Pages, frontEndOptions.PagePriority);

            frontEndOptions.Validate();
            pageOptions.Validate();
        }

        private void PrioritizePages(List<string> pages, List<HealthCheckPageType> priority)
        {
            foreach(var prio in priority.Reverse<HealthCheckPageType>().Select(x => GetPageTypeString(x)))
            {
                if (pages.Contains(prio))
                {
                    pages.Remove(prio);
                    pages.Insert(0, prio);
                }
            }
        }

        private string GetPageTypeString(HealthCheckPageType type)
            => Pages.FirstOrDefault(x => x.Type == type)?.Id
                ?? throw new NotImplementedException($"Page type {type.ToString()} not fully implemented yet.");

        private List<TestClassDefinition> GetTestDefinitions(Maybe<TAccessRole> accessRoles)
        {
            var userRolesMaybe = accessRoles ?? new Maybe<TAccessRole>();
            var userRoles = userRolesMaybe.HasValue ? (object)userRolesMaybe.Value : null;
            return TestDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: userRoles);
        }
        
        private TestDefinition GetTest(Maybe<TAccessRole> accessRoles, string testId)
            => GetTestDefinitions(accessRoles).SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);

        internal bool CanShowPageTo(HealthCheckPageType type, Maybe<TAccessRole> accessRoles)
            => Pages.FirstOrDefault(x => x.Type == type)?.IsVisible(this, accessRoles) == true;

        /// <summary>
        /// Default value if pageAccess is null, false if no roles were given.
        /// </summary>
        private bool AccessRolesHasAccessTo(Maybe<TAccessRole> accessRoles, Maybe<TAccessRole> pageAccess, bool defaultValue = true)
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

        #region Log search
        private class LogSearchInProgress
        {
            public string Id { get; set; }
            public DateTime StartedAt { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }

        private static List<LogSearchInProgress> SearchesInProgress { get; set; } = new List<LogSearchInProgress>();
        private int AbortLogSearches(DateTime? threshold = null)
        {
            lock (SearchesInProgress)
            {
                var searchesToCleanup = SearchesInProgress
                    .Where(x => threshold == null || x.StartedAt < threshold).ToList();

                foreach (var search in searchesToCleanup)
                {
                    CancelLogSearch(search.Id);
                }

                return searchesToCleanup.Count;
            }
        }
        #endregion

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
        /// When data has been fetched from a datastream this should be called.
        /// </summary>
        public void AuditLog_SettingsSaved(RequestInformation<TAccessRole> requestInformation, IEnumerable<HealthCheckSetting> settings)
        {
            string settingsString = JsonConvert.SerializeObject(settings.Select(x => new { Id = x.Id, Value = x.Value }));

            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.Settings, action: "Settings updated", subject: "Settings")
                .AddDetail("Values", settingsString)
            );
        }

        /// <summary>
        /// When data has been fetched from a datastream this should be called.
        /// </summary>
        public void AuditLog_DatastreamFetched(RequestInformation<TAccessRole> requestInformation,
            DataflowStreamMetadata<TAccessRole> stream, DataflowStreamFilter filter)
        {
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.Dataflow, action: "Dataflow stream fetched", subject: stream?.Name)
                .AddDetail("Stream id", stream?.Id)
                .AddDetail("Filter input", (filter == null) ? null : SerializeJson(filter))
            );
        }

        /// <summary>
        /// When a test has executed this should be called.
        /// </summary>
        public void AuditLog_TestExecuted(RequestInformation<TAccessRole> requestInformation, ExecuteTestInputData input, TestResultViewModel result)
        {
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.Tests, action: "Test executed", subject: result?.TestName)
                .AddDetail("Test id", input?.TestId)
                .AddDetail("Parameters", $"[{string.Join(", ", (input?.Parameters ?? new List<string>()))}]")
                .AddDetail("Result", result?.Message)
                .AddDetail("Duration", $"{result?.DurationInMilliseconds}ms")
            );
        }

        /// <summary>
        /// When a log search has been started this should be called.
        /// </summary>
        public void AuditLog_LogSearch(RequestInformation<TAccessRole> requestInformation, LogSearchFilter filter, LogSearchResult result)
        {
            if (result.WasCancelled)
                return;

            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.LogSearch, action: "Searched logs", subject: filter?.Query)
                .AddDetail("Skip", filter?.Skip.ToString() ?? "null")
                .AddDetail("Take", filter?.Take.ToString() ?? "null")
                .AddDetail("Range", $"{filter?.FromDate?.ToString() ?? "min"} -> {filter?.ToDate?.ToString() ?? "max"}")
                .AddDetail("Result count", result?.Items?.Count.ToString() ?? "null")
                .AddDetail("Duration", $"{result?.DurationInMilliseconds}ms")
            );
        }

        /// <summary>
        /// When a log search has been started this should be called.
        /// </summary>
        public void AuditLog_LogSearchCancel(RequestInformation<TAccessRole> requestInformation, string action, int? count = null)
        {
            Services.AuditEventService?.StoreEvent(
                CreateAuditEventFor(requestInformation, AuditEventArea.LogSearch, action: action)
                    .AddDetail("Count", count?.ToString(), onlyIfNotNull: true)
            );
        }

        /// <summary>
        /// Get viewmodel for audit filter results.
        /// </summary>
        public async Task<IEnumerable<AuditEventViewModel>> GetAuditEventsFilterViewModel(
            Maybe<TAccessRole> accessRoles,
            AuditEventFilterInputData filter)
        {
            if (Services.AuditEventService == null || !RoleHasAccessToAuditLogs(accessRoles))
                return Enumerable.Empty<AuditEventViewModel>();

            var from = filter?.FromFilter ?? DateTime.MinValue;
            var to = filter?.ToFilter ?? DateTime.MaxValue;
            var events = await Services.AuditEventService.GetEvents(from, to);
            return events
                .Where(x => AuditEventMatchesFilter(x, filter))
                .Select(x => TestsViewModelsFactory.CreateViewModel(x));
        }

        /// <summary>
        /// Get viewmodel for dataflow entries result.
        /// </summary>
        public async Task<IEnumerable<IDataflowEntry>> GetDataflowEntries(string streamId, DataflowStreamFilter filter,
            RequestInformation<TAccessRole> requestInformation)
        {
            if (Services.DataflowService == null 
                || !GetDataflowStreamsMetadata(requestInformation.AccessRole).Any())
                return Enumerable.Empty<IDataflowEntry>();

            var stream = this.GetDataflowStreamsMetadata(requestInformation.AccessRole)
                .FirstOrDefault(x => x.Id == streamId);
            if (stream != null)
            {
                AuditLog_DatastreamFetched(requestInformation, stream, filter);
            }

            filter = filter ?? new DataflowStreamFilter();
            filter.PropertyFilters = filter.PropertyFilters ?? new Dictionary<string, string>();
            return await Services.DataflowService.GetEntries(streamId, filter);
        }

        /// <summary>
        /// Get viewmodel for dataflow streams metadata result.
        /// </summary>
        public IEnumerable<DataflowStreamMetadata<TAccessRole>> GetDataflowStreamsMetadata(Maybe<TAccessRole> accessRoles)
        {
            if (Services.DataflowService == null || !CanShowDataflowPageTo(accessRoles, checkCount: false))
                return Enumerable.Empty<DataflowStreamMetadata<TAccessRole>>();

            return Services.DataflowService.GetStreamMetadata()
                .Where(x => AccessRolesHasAccessTo(accessRoles, x.RolesWithAccess, defaultValue: true));
        }

        /// <summary>
        /// Get viewmodel for dataflow streams metadata result.
        /// </summary>
        public GetSettingsViewModel GetSettings(Maybe<TAccessRole> accessRoles)
        {
            if (Services.SettingsService == null || !CanShowPageTo(HealthCheckPageType.Settings, accessRoles))
                return new GetSettingsViewModel();

            var settings = Services.SettingsService.GetSettingItems();
            return new GetSettingsViewModel()
            {
                Settings = settings
            };
        }

        /// <summary>
        /// Save the given settings.
        /// </summary>
        public void SetSettings(RequestInformation<TAccessRole> requestInformation, SetSettingsViewModel model)
        {
            if (Services.SettingsService == null || !CanShowPageTo(HealthCheckPageType.Settings, requestInformation.AccessRole))
                return;

            AuditLog_SettingsSaved(requestInformation, model.Settings);
            Services.SettingsService.SaveSettings(model.Settings);
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

        #region Helper classes
        internal class PageType
        {
            public string Id { get; set; }
            public HealthCheckPageType Type { get; set; }
            public Func<HealthCheckControllerHelper<TAccessRole>, Maybe<TAccessRole>, bool> IsVisible { get; set; }
            public Action<FrontEndOptionsViewModel> OnAccessDenied { get; set; }

            public PageType(string id, HealthCheckPageType type,
                Func<HealthCheckControllerHelper<TAccessRole>, Maybe<TAccessRole>, bool> isVisible,
                Action<FrontEndOptionsViewModel> onAccessDenied = null)
            {
                Id = id;
                Type = type;
                IsVisible = isVisible;
                OnAccessDenied = onAccessDenied;
            }
        }
        #endregion
    }
}
