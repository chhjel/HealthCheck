using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Services.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCheck.Core.Services
{
    public class TestRunnerTests
    {
        [Fact]
        public async Task ExecuteTests_WithFilter_OnlyExecutesMatchingTests()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var runner = new TestRunnerService();
            var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("CategoryA"));
            Assert.Single(results);
            Assert.Contains("CategoryA", results.Single().Test.Categories);
        }

        [Fact]
        public async Task ExecuteTests_WithEventService_StoresEventResultsInService()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var runner = new TestRunnerService();
            var eventService = new MemorySiteEventService();
            var results = await runner.ExecuteTests(discoverer, (test) => true, siteEventService: eventService);
            Assert.Contains(results, result => result.SiteEvent?.Title == "EventA");

            var events = await eventService.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Contains(events, e => e.Title == "EventA");
        }

        [Fact]
        public async Task ExecuteTests_WithAuditService_StoresAuditEventsForExecutedTestsInService()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var runner = new TestRunnerService();
            var auditService = new MemoryAuditEventService();
            var results = await runner.ExecuteTests(discoverer, (test) => true, auditEventService: auditService);
            Assert.Contains(results, result => result.SiteEvent?.Title == "EventA");

            var events = await auditService.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Contains(events, e => e.Subject == "TestMethodA");
        }

        [Fact]
        public async Task ExecuteTests_WithAFewTests_ReturnsResults()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var testClasses = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetA")
                .ToList();

            var runner = new TestRunnerService();
            var results = await runner.ExecuteTests(testClasses);
            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ExecuteTests_WithAsyncAndTaskTest_DoesNotFail()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var testClasses = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetB")
                .ToList();

            var runner = new TestRunnerService();
            var results = await runner.ExecuteTests(testClasses);
            Assert.NotEmpty(results);
            Assert.Contains(results, x => x.Tag as string == "AsyncTestMethod");
            Assert.Contains(results, x => x.Tag as string == "TaskTestMethod");
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetA", Description = "Some test set", Name = "Dev test set")]
        public class TestClassA
        {
            [RuntimeTest]
            public TestResult TestMethodWithoutParameters()
            {
                return new TestResult();
            }

            [RuntimeTest(ParameterDescriptions = new[] { "a desc", "b", "c" })]
            public TestResult TestMethodWithParameters(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                return new TestResult()
                {
                    Tag = new object[] { stringArg, boolArg, intArg }
                };
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetB", Description = "Some test set", Name = "Dev test set")]
        public class TestClassB
        {
            [RuntimeTest(Category = "CategoryB")]
            public async Task<TestResult> AsyncTestMethod()
            {
                await Task.Delay(100);
                return new TestResult() { Tag = "AsyncTestMethod" };
            }

            [RuntimeTest(Category = "CategoryA")]
            public Task<TestResult> TaskTestMethod()
            {
                var e = new SiteEvent(Enums.SiteEventSeverity.Error, "typeId", "EventA", "description");
                return Task.FromResult(new TestResult() { Tag = "TaskTestMethod" }
                    .SetSiteEvent(e));
            }
        }
    }
}
