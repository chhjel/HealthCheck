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
            var eventService = new SiteEventService(new MemorySiteEventStorage());
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
            var auditService = new MemoryAuditEventStorage();
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

        [Fact]
        public async Task ExecuteTest_WithDefaultValueAnInput_UsesInput()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var test = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetC")
                .SelectMany(x => x.Tests)
                .FirstOrDefault(x => x.Name == "Test With Default Values");

            var runner = new TestRunnerService();
            var result = await runner.ExecuteTest(test, new object[] { "new value" }, allowDefaultValues: true);
            Assert.Equal("new value", result.Tag);
        }

        [Fact]
        public async Task ExecuteTest_WithDefaultValueAndNullInput_UsesNullInput()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var test = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetC")
                .SelectMany(x => x.Tests)
                .FirstOrDefault(x => x.Name == "Test With Default Values");

            var runner = new TestRunnerService();
            var result = await runner.ExecuteTest(test, new object[] { null }, allowDefaultValues: false);
            Assert.Null(result.Tag);
        }

        [Fact]
        public async Task ExecuteTests_TestsWithoutDefaultValue_ShouldUseTypeDefaultValues()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var testClasses = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetC")
                .ToList();

            var runner = new TestRunnerService();
            var results = await runner.ExecuteTests(testClasses, (x) => x.Name == "Test Without Default Values");
            Assert.Single(results);

            var data = results[0].Tag as object[];
            Assert.Equal(default, data[0] as string);
            Assert.Equal(default, (DateTime)data[1]);
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetA", Description = "Some test set", Name = "Dev test set")]
        public class TestClassA
        {
            [RuntimeTest]
            public TestResult TestMethodWithoutParameters()
            {
                return new TestResult();
            }

            [RuntimeTest()]
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

        [RuntimeTestClass(Id = "TestRunnerTestsSetC", Description = "Some test set", Name = "Dev test set")]
        public class TestClassC
        {
            [RuntimeTest(Category = "CategoryC")]
            public TestResult TestWithDefaultValues(string text = "defaultValue")
            {
                return new TestResult() { Tag = text };
            }

            [RuntimeTest(Category = "CategoryD")]
            public TestResult TestWithoutDefaultValues(string text, DateTime date)
            {
                return new TestResult() { Tag = new object[] { text, date } };
            }
        }
    }
}
