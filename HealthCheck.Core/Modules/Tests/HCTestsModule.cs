using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Config;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.Tests.Factories;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Module for executing tests at runtime.
    /// </summary>
    public class HCTestsModule : HealthCheckModuleBase<HCTestsModule.AccessOption>
    {
        /// <inheritdoc />
        public override List<string> AllCategories => _allCategories;

        /// <inheritdoc />
        public override List<HCModuleIdData> AllIds => _allIds;

        /// <summary>
        /// Handles conversion from stringified test input to parameter types.
        /// </summary>
        public HCStringConverter ParameterConverter { get; } = new HCStringConverter();

        private TestRunnerService TestRunner { get; } = new TestRunnerService();
        private TestDiscoveryService TestDiscoverer { get; }
        private TestViewModelsFactory TestsViewModelsFactory { get; } = new TestViewModelsFactory();
        private TestSetGroupsOptions GroupOptions { get; } = new TestSetGroupsOptions();
        private static List<RuntimeTestReferenceParameterFactory> _referenceParameterFactories;
        private static readonly object _referenceParameterFactoriesLock = new();
        private static readonly object _ensureDataLock = new();
        private static List<string> _allCategories;
        private static List<HCModuleIdData> _allIds;

        /// <summary>
        /// Options model for this module.
        /// </summary>
        protected readonly HCTestsModuleOptions Options;

        /// <summary>
        /// Module for executing tests at runtime.
        /// </summary>
        public HCTestsModule(HCTestsModuleOptions options)
        {
            var assemblies = options.AssembliesContainingTests;
            if (assemblies == null || !assemblies.Any())
            {
                throw new ArgumentException(
                    $"{nameof(HCTestsModuleOptions.AssembliesContainingTests)} must contain at least one assembly to retrieve tests from.");
            }

            EnsureReferenceParameterFactoriesInitialized(options);

            TestDiscoverer = new TestDiscoveryService()
            {
                AssembliesContainingTests = assemblies,
                ReferenceParameterFactories = _referenceParameterFactories,
                IncludeProxyTests = options.IncludeProxyTests
            };
            Options = options;

            EnsureDataInitialized();
            lock (AllowedDownloadsCache)
            {
                AllowedDownloadsCache.RemoveExpired(TimeSpan.FromMinutes(5));
            }
        }

        private static void EnsureReferenceParameterFactoriesInitialized(HCTestsModuleOptions options)
        {
            lock (_referenceParameterFactoriesLock)
            {
                _referenceParameterFactories ??= options.ReferenceParameterFactories?.Invoke()
                    ?? new List<RuntimeTestReferenceParameterFactory>();
            }
        }

        private void EnsureDataInitialized()
        {
            lock (_ensureDataLock)
            {
                List<TestClassDefinition> allTests = null;

                if (_allCategories == null)
                {
                    allTests ??= TestDiscoverer.DiscoverTestDefinitions();
                    _allCategories = allTests
                        .SelectMany(x => x.Tests).SelectMany(x => x.Categories)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToList();
                }

                if (_allIds == null)
                {
                    allTests ??= TestDiscoverer.DiscoverTestDefinitions();
                    _allIds = allTests
                        .SelectMany(x => x.Tests).Select(x => new HCModuleIdData { Id = x.Id, Name = x.Name })
                        .OrderBy(x => x.Name)
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context)
            => new { AllowAnyParameterType = Options.AllowAnyParameterType };

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCTestsModuleConfig();

        /// <summary>
        /// Configure grouping.
        /// </summary>
        public HCTestsModule ConfigureGroups(Action<TestSetGroupsOptions> optionsModifier)
        {
            optionsModifier(GroupOptions);
            return this;
        }

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>Gives access to view the module.</summary>
            ViewInvalidTests = 1,

            /// <summary>Gives access to view stacktrace if an unhandled exception is thrown during test execution.</summary>
            IncludeExceptionStackTraces = 2
        }

        #region Invokable methods
        /// <summary>
        /// Get all available tests for the current request.
        /// </summary>
        [HealthCheckModuleMethod]
        public TestsDataViewModel GetTests(HealthCheckModuleContext context)
        {
            var invalidTests = new List<TestDefinitionValidationResult>();
            if (context.HasAccess(AccessOption.ViewInvalidTests))
            {
                invalidTests = TestDiscoverer.GetInvalidTests();
            }

            TestDiscoverer.GetInvalidTests();

            var testDefinitions = GetTestDefinitions(context.CurrentRequestRoles, context.CurrentModuleCategoryAccess, context.CurrentModuleIdAccess);
            var model = new TestsDataViewModel()
            {
                TestSets = TestsViewModelsFactory.CreateViewModel(testDefinitions, Options, context.CurrentModuleCategoryAccess),
                GroupOptions = TestsViewModelsFactory.CreateViewModel(GroupOptions),
                InvalidTests = invalidTests.Select(x => (TestsViewModelsFactory.CreateViewModel(x))).ToList(),
                ParameterTemplateValues = TestsViewModelsFactory.CreateParameterTemplatesViewModel(testDefinitions, Options)
            };
            return model;
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<object> ExecuteTest(HealthCheckModuleContext context, ExecuteTestInputData data)
        {
            var result = await ExecuteTest(context.CurrentRequestRoles, context.CurrentModuleCategoryAccess, context.CurrentModuleIdAccess,
                data, context.HasAccess(AccessOption.IncludeExceptionStackTraces));

            IEnumerable<string> parameters = null;
            if (result.InputWasAllowedAuditLogged)
            {
                parameters = data?.Parameters?.Select(x => context.TryStripSensitiveData(x.Value))
                    ?? Enumerable.Empty<string>();
            }
            string testOutput = null;
            if (result.ResultMessageWasAllowedAuditLogged)
            {
                testOutput = context.TryStripSensitiveData(result?.Message);
            }

            context.AddAuditEvent(action: "Test executed", subject: result?.TestName)
                .AddDetail("Test id", data?.TestId)
                .AddDetail("Parameters", parameters == null ? null : $"[{string.Join(", ", parameters)}]", true)
                .AddDetail("Result", testOutput, true)
                .AddDetail("Duration", $"{result?.DurationInMilliseconds}ms");

            return result;
        }

        /// <summary>
        /// Execute tests in the given category.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<ExecuteTestsResult> ExecuteTests(HealthCheckModuleContext context, ExecuteTestsInputData data)
        {
            var result = await ExecuteTests(
                context,
                testFilter: (test) =>
                    test.Categories.Contains(data.TestCategory)
                    && EnumUtils.EnumFlagHasAnyFlagsSet(context.CurrentRequestRoles, test.RolesWithAccess),
                testCategory: data.TestCategory
            );
            return result;
        }

        /// <summary>
        /// Requests cancellation of the given cancellable test.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<bool> CancelTest(HealthCheckModuleContext context, string testId)
        {
            if (testId == null)
            {
                return false;
            }

            var test = GetTest(context.CurrentRequestRoles, context.CurrentModuleCategoryAccess, context.CurrentModuleIdAccess, testId);
            if (test == null)
            {
                return false;
            }

            var registered = TestRunner.RequestTestCancellation(testId);
            if (registered)
            {
                context.AddAuditEvent(action: "Test cancellation requested", subject: test?.Name)
                    .AddDetail("Test id", test?.Id);
            }
            return await Task.FromResult(registered);
        }

        /// <summary>
        /// Get options for a given reference parameter.
        /// </summary>
        [HealthCheckModuleMethod]
        public IEnumerable<RuntimeTestReferenceParameterChoice> GetReferenceParameterOptions(HealthCheckModuleContext context, GetReferenceParameterOptionsRequestModel data)
        {
            var testDefinitions = GetTestDefinitions(context.CurrentRequestRoles, context.CurrentModuleCategoryAccess, context.CurrentModuleIdAccess);
            var test = testDefinitions.SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == data.TestId);
            var parameter = test?.Parameters?.FirstOrDefault(x => x.Index == data.ParameterIndex);
            if (parameter == null)
            {
                return Enumerable.Empty<RuntimeTestReferenceParameterChoice>();
            }

            var type = parameter.ParameterType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            var factory = parameter.GetParameterFactory(test);
            return factory.GetChoicesFor(type, data.Filter)
                ?? Enumerable.Empty<RuntimeTestReferenceParameterChoice>();
        }
        #endregion

        #region Actions
        private static readonly Regex _downloadFileUrlRegex
            = new(@"^/TMDownloadFile/(?<type>[\w-]+)?__(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);

        /// <summary>
        /// Download a file.
        /// </summary>
        [HealthCheckModuleAction]
        public virtual object TMDownloadFile(HealthCheckModuleContext context, string url)
        {
            var match = _downloadFileUrlRegex.Match(url);
            if (!match.Success)
            {
                return null;
            }

            // Parse url
            var idFromUrl = match.Groups["id"].Value.Trim();
            var typeUrlMatch = match.Groups["type"];
            string typeFromUrl = typeUrlMatch.Success ? typeUrlMatch.Value : null;

            if (!IsFileDownloadAllowedForSession(typeFromUrl, idFromUrl))
            {
                return null;
            }
            else if (Options.FileDownloadHandler == null)
            {
                return HealthCheckFileDownloadResult.CreateFromString("not_configured.txt",
                    $"FileDownloadHandler has not been configured. Please set {nameof(HCTestsModuleOptions)}.{nameof(HCTestsModuleOptions.FileDownloadHandler)}.");
            }

            var file = Options.FileDownloadHandler?.Invoke(typeFromUrl, idFromUrl);
            if (file == null)
            {
                return null;
            }

            // Store audit data
            context.AddAuditEvent("File download", file.FileName)
                .AddClientConnectionDetails(context)
                .AddDetail("File Name", file.FileName);

            return file;
        }
        #endregion

        #region Internal helpers
        /// <summary>
        /// Stores the last 100 allowed download type/ids.
        /// </summary>
        protected static readonly HCSimpleMemoryCache<List<string[]>> AllowedDownloadsCache = new()
        {
            MaxCount = 100,
            DefaultDuration = TimeSpan.FromMinutes(10)
        };
        internal static void AllowFileDownloadForSession(string type, string id)
        {
            lock (AllowedDownloadsCache)
            {
                try
                {
                    var sessionId = HCGlobalConfig.GetCurrentSessionId?.Invoke();
                    if (string.IsNullOrWhiteSpace(sessionId))
                    {
                        return;
                    }

                    var key = $"{sessionId}__{type}__{id}";
                    var maxLimitPerSession = 100;
                    var list = AllowedDownloadsCache[key] ?? new List<string[]>();
                    if (list.Any(x => x[0] == type && x[1] == id))
                    {
                        return;
                    }

                    list.Add(new[] { type, id });
                    if (list.Count > maxLimitPerSession)
                    {
                        list.RemoveAt(0);
                    }

                    AllowedDownloadsCache[key] = list;
                }
                catch (Exception) { /* Ignored */ }
            }
        }

        /// <summary>
        /// Checks if session contains the type and id combination.
        /// </summary>
        protected virtual bool IsFileDownloadAllowedForSession(string type, string id)
        {
            lock (AllowedDownloadsCache)
            {
                try
                {
                    var sessionId = HCGlobalConfig.GetCurrentSessionId?.Invoke();
                    if (string.IsNullOrWhiteSpace(sessionId))
                    {
                        return false;
                    }

                    var key = $"{sessionId}__{type}__{id}";
                    var list = AllowedDownloadsCache[key];
                    return list?.Any(x => x[0] == type && x[1] == id) == true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        #endregion

        #region Private helpers
        private List<TestClassDefinition> GetTestDefinitions(object currentRequestRoles, List<string> userCategoryAccess, List<string> userIdAccess)
            => TestDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true,
                userRolesEnum: currentRequestRoles, defaultTestAccessLevel: Options.DefaultTestAccessLevel,
                userCategoryAccess: userCategoryAccess, userIdAccess: userIdAccess);

        private TestDefinition GetTest(object currentRequestRoles, List<string> userCategoryAccess, List<string> userIdAccess, string testId)
            => GetTestDefinitions(currentRequestRoles, userCategoryAccess, userIdAccess)
                .SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);

        private async Task<TestResultViewModel> ExecuteTest(object accessRoles, List<string> categoryAccess, List<string> userIdAccess, ExecuteTestInputData data,
            bool includeExceptionStackTraces)
        {
            if (data == null || data.TestId == null)
            {
                return TestResultViewModel.CreateError("No test id was given.", null, "<Unknown>");
            }

            var test = GetTest(accessRoles, categoryAccess, userIdAccess, data.TestId);
            if (test == null)
            {
                return TestResultViewModel.CreateError($"Test with id '{data.TestId}' not found.", data.TestId, "<Unknown>");
            }

            try
            {
                var parameters = data?.GetParametersWithConvertedTypes(test.Parameters.Select(x => x.ParameterType).ToArray(), ParameterConverter, test);
                var result = await TestRunner.ExecuteTest(test, parameters,
                    allowDefaultValues: false,
                    includeExceptionStackTraces: includeExceptionStackTraces,
                    resultAction: Options.AutoResultAction
                );
                return TestsViewModelsFactory.CreateViewModel(result);
            }
            catch (Exception ex)
            {
                var message = $"Exception: {(ex.InnerException ?? ex).Message}";
                return TestResultViewModel.CreateError(message, test.Id, test.Name);
            }
        }

        private async Task<ExecuteTestsResult> ExecuteTests(HealthCheckModuleContext context, Func<TestDefinition, bool> testFilter, string testCategory)
        {
            if (testCategory == null)
            {
                return new ExecuteTestsResult { TotalResult = TestResultStatus.Error, ErrorMessage = "No category to test was given." };
            }

            try
            {
                var results = await TestRunner.ExecuteTests(TestDiscoverer,
                    testFilter: t => testFilter(t)
                        && (context.CurrentModuleIdAccess?.Any() != true || context.CurrentModuleIdAccess.Contains(t.Id))
                        && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => t.Categories?.Contains(c) == true)),
                    onAuditEvent: (e) => Task.FromResult(context.AddAuditEvent(e)),
                    auditUserId: context.UserId,
                    auditUsername: context.UserName
                );

                var includeExceptionStackTraces = context.HasAccess(AccessOption.IncludeExceptionStackTraces);
                var testResults = results.Select(x => new ExecuteTestsTestResult()
                {
                    TestId = x.Test?.Id,
                    TestName = x.Test?.Name,
                    Result = x.Status,
                    Message = x.Message,
                    StackTrace = includeExceptionStackTraces ? x.StackTrace : null
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
        #endregion
    }
}
