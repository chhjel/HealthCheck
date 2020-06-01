using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Tests.Factories;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Module for executing tests at runtime.
    /// </summary>
    public class HCTestsModule : HealthCheckModuleBase<HCTestsModule.AccessOption>
    {
        /// <summary>
        /// Module for executing tests at runtime.
        /// </summary>
        public HCTestsModule(HCTestsModuleOptions options)
        {
            TestDiscoverer.AssemblyContainingTests = options.AssemblyContainingTests
                ?? throw new ArgumentNullException($"[{nameof(HCTestsModuleOptions)}.{nameof(HCTestsModuleOptions.AssemblyContainingTests)}] must be set to an assembly to retrieve tests from must be provided.");
        }

        /// <summary>
        /// Handles conversion from stringified test input to parameter types.
        /// </summary>
        public StringConverter ParameterConverter { get; } = new StringConverter();

        private readonly TestRunnerService TestRunner = new TestRunnerService();
        private readonly TestDiscoveryService TestDiscoverer = new TestDiscoveryService();
        private readonly TestViewModelsFactory TestsViewModelsFactory = new TestViewModelsFactory();
        private readonly TestSetGroupsOptions GroupOptions = new TestSetGroupsOptions();

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

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
            Nothing = 0,

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
            var testDefinitions = GetTestDefinitions(context.CurrentRequestRoles);
            var model = new TestsDataViewModel()
            {
                TestSets = TestsViewModelsFactory.CreateViewModel(testDefinitions),
                GroupOptions = TestsViewModelsFactory.CreateViewModel(GroupOptions),
                InvalidTests = invalidTests.Select(x => (TestsViewModelsFactory.CreateViewModel(x))).ToList()
            };
            return model;
        }

        /// <summary>
        /// Execute the given test.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<object> ExecuteTest(HealthCheckModuleContext context, ExecuteTestInputData data)
        {
            var result = await ExecuteTest(context.CurrentRequestRoles, data, context.HasAccess(AccessOption.IncludeExceptionStackTraces));
            context.AddAuditEvent(action: "Test executed", subject: result?.TestName)
                .AddDetail("Test id", data?.TestId)
                .AddDetail("Parameters", $"[{string.Join(", ", (data?.Parameters ?? new List<string>()))}]")
                .AddDetail("Result", result?.Message)
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

            var test = GetTest(context.CurrentRequestRoles, testId);
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
        #endregion

        #region Private helpers
        private List<TestClassDefinition> GetTestDefinitions(object currentRequestRoles)
            => TestDiscoverer.DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: currentRequestRoles);

        private TestDefinition GetTest(object currentRequestRoles, string testId)
            => GetTestDefinitions(currentRequestRoles).SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);

        private async Task<TestResultViewModel> ExecuteTest(object accessRoles, ExecuteTestInputData data, bool includeExceptionStackTraces)
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
                var result = await TestRunner.ExecuteTest(test, parameters,
                    allowDefaultValues: false,
                    includeExceptionStackTraces: includeExceptionStackTraces
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
                    testFilter: testFilter,
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
