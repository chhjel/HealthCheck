using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Services
{
    /// <summary>
    /// Executes tests.
    /// </summary>
    public class TestRunnerService
    {
        /// <summary>
        /// Include exception stacktraces in test results.
        /// <para>False by default.</para>
        /// </summary>
        public bool IncludeExceptionStackTraces { get; set; }

        /// <summary>
        /// Executes matching tests.
        /// <para>Optionally sends results containing a <see cref="TestResult.SiteEvent"/> if a <see cref="ISiteEventStorage"/> is provided,
        /// and creates audit events if a <see cref="IAuditEventStorage"/> is provided.</para>
        /// <para><see cref="TestResult.SiteEvent"/> marked as resolved will resolve the last event with a matching event type id.</para>
        /// </summary>
        /// <param name="testDiscoveryService">Service that will discover tests to run.</param>
        /// <param name="testFilter">Only tests that return true to this condition will be executed.</param>
        /// <param name="siteEventService">Service that any <see cref="TestResult.SiteEvent"/> will be sent to if not null.</param>
        /// <param name="auditEventService">Stores an entry to the audit service if not null.</param>
        /// <param name="auditUserId">User id stored in audit log if service is provided.</param>
        /// <param name="auditUsername">User name stored in audit log if service is provided.</param>
        /// <param name="multipleResultSiteEventMergeLogic">
        /// Merge logic used when multiple <see cref="SiteEvent"/>s have the same eventTypeId.
        /// The parameter is a list of all the reported unresolved site events, and the return value should be the merged event to store/merge with previously stored events.
        /// Uses <see cref="DefaultMultipleResultSiteEventMerge"/> by default if left null. If a Maybe(null) is given merge will be disabled.
        /// The default logic uses the highest severity event, and appends descriptions from any other one.
        /// </param>
        /// <returns>All executed test results are returned.</returns>
        public async Task<List<TestResult>> ExecuteTests(
            TestDiscoveryService testDiscoveryService,
            Func<TestDefinition, bool> testFilter,
            ISiteEventService siteEventService = null,
            IAuditEventStorage auditEventService = null,
            string auditUserId = "0",
            string auditUsername = "System",
            Maybe<Func<IEnumerable<SiteEvent>, SiteEvent>> multipleResultSiteEventMergeLogic = null)
        {
            var tests = testDiscoveryService.DiscoverTestDefinitions(includeInvalidTests: false, onlyTestsAllowedToBeManuallyExecuted: false, testFilter: testFilter);
            var results = await ExecuteTests(tests, testFilter);

            if (auditEventService != null)
            {
                foreach(var result in results)
                {
                    await auditEventService.StoreEvent(new AuditEvent()
                        {
                            Area = Enums.AuditEventArea.Tests,
                            Action = "Test executed",
                            Subject = result?.Test?.Name,
                            Timestamp = DateTime.Now,
                            UserId = auditUserId,
                            UserName = auditUsername,
                            UserAccessRoles = new List<string>()
                        }
                        .AddDetail("Test id", result?.Test?.Id)
                        .AddDetail("Result", result?.Message)
                        .AddDetail("Duration", (result == null ? "?" : result.DurationInMilliseconds < 0 ? "< 0" : result?.DurationInMilliseconds.ToString()) + "ms")
                    );
                }
            }

            if (siteEventService != null)
            {
                var siteEvents = results
                    .Where(x => x.SiteEvent != null)
                    .Select(x => x.SiteEvent)
                    .ToList();

                var includedSiteEvents = siteEvents
                    .Where(x => !x.Resolved || !ContainsAnyUnresolvedTestsFor(x, siteEvents))
                    .ToList();

                var mergeLogic = (multipleResultSiteEventMergeLogic != null) ? multipleResultSiteEventMergeLogic.Value : DefaultMultipleResultSiteEventMerge;
                if (mergeLogic != null)
                {
                    includedSiteEvents = includedSiteEvents
                        .GroupBy(x => x.EventTypeId)
                        .Select(x => DefaultMultipleResultSiteEventMerge(x))
                        .ToList();
                }

                foreach (var ev in includedSiteEvents)
                {
                    if (ev.Resolved)
                    {
                        await siteEventService.MarkEventAsResolved(ev.EventTypeId, ev.ResolvedMessage);
                    } else
                    {
                        await siteEventService.StoreEvent(ev);
                    }
                }
            }

            return results;
        }

        private bool ContainsAnyUnresolvedTestsFor(SiteEvent self, List<SiteEvent> events)
        {
            return events.Any(other =>
                other != self
                && other.EventTypeId == self.EventTypeId
                && other.Resolved == false
            );
        }

        /// <summary>
        /// Default merge logic for the ExecuteTests method.
        /// <para>Uses the highest severity event, and appends descriptions from any other one.</para>
        /// </summary>
        /// <param name="events">Events with the same event type id.</param>
        public static SiteEvent DefaultMultipleResultSiteEventMerge(IEnumerable<SiteEvent> events)
        {
            var ordered = events.OrderByDescending(x => x.Severity);

            var mainEvent = ordered.First();
            var otherEvents = ordered.Where(x => x != mainEvent);

            var otherDesc = string.Join("\n\n", otherEvents.Select(x => x.Description));
            if (!string.IsNullOrWhiteSpace(otherDesc))
            {
                mainEvent.Description += "\n\n" + otherDesc;
            }

            return mainEvent;
        }

        /// <summary>
        /// Execute all the tests in the given test classes.
        /// </summary>
        /// <param name="testClasses">Classes to execute.</param>
        /// <param name="testFilter">Only tests that return true to this condition will be executed. If null all tests will be included.</param>
        public async Task<List<TestResult>> ExecuteTests(
            List<TestClassDefinition> testClasses,
            Func<TestDefinition, bool> testFilter = null)
        {
            var results = new ConcurrentBag<TestResult>();
            foreach (var testClass in testClasses)
            {
                var classInstance = Activator.CreateInstance(testClass.ClassType);
                var includedTests = testClass.Tests.Where(x => testFilter?.Invoke(x) != false).ToList();

                var defaultAllowsParallel = testClass.DefaultAllowParallelExecution == true;
                var testsThatCanRunInParallel = includedTests
                    .Where(x =>
                        (defaultAllowsParallel
                        && (x.AllowParallelExecution == null || x.AllowParallelExecution == true))
                        || (!defaultAllowsParallel
                        && (x.AllowParallelExecution != null && x.AllowParallelExecution == true))
                    );
                var testsThatCannotRunInParallel = includedTests.Except(testsThatCanRunInParallel);

                // Run tests that are allowed to run in parallel first
                if (testsThatCanRunInParallel.Any())
                {
                    var tasks = new List<Task<TestResult>>();
                    foreach (var test in testsThatCanRunInParallel)
                    {
                        var task = ExecuteTest(test, null, classInstance, allowDefaultValues: true);
                        tasks.Add(task);
                    }
                    await Task.WhenAll(tasks);
                }

                // Run other tests after
                foreach (var test in testsThatCannotRunInParallel)
                {
                    var result = await ExecuteTest(test, null, classInstance, allowDefaultValues: true);
                    results.Add(result);
                }
            }
            return results.ToList();
        }

        /// <summary>
        /// Execute the given test with optional parameters and instance.
        /// </summary>
        public async Task<TestResult> ExecuteTest(TestDefinition test,
            object[] parameters = null,
            object testClassInstance = null,
            bool allowDefaultValues = true)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var instance = testClassInstance ?? Activator.CreateInstance(test.ParentClass.ClassType);
                var result = await test.ExecuteTest(instance, parameters, allowDefaultValues);

                // Post-process result
                result.Test = test;
                result.DurationInMilliseconds = stopWatch.ElapsedMilliseconds;
                if (!IncludeExceptionStackTraces)
                {
                    result.StackTrace = null;
                }

                return result;
            }
            catch (Exception ex)
            {
                return new TestResult()
                {
                    Test = test,
                    Status = Enums.TestResultStatus.Error,
                    Message = $"Failed to execute test with the exception: {ex.Message}",
                    StackTrace = IncludeExceptionStackTraces ? ex.ToString() : null,
                    DurationInMilliseconds = stopWatch.ElapsedMilliseconds
            };
            }
        }
    }
}
