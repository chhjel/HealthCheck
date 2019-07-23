using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Services.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.Core.Services
{
    public class TestRunnerTests
    {
        public ITestOutputHelper Output { get; }

        public TestRunnerTests(ITestOutputHelper output)
        {
            Output = output;
        }

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
        public async Task ExecuteTests_WithEventService_CanResolveLastMatchingEvent()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var runner = new TestRunnerService();
            var eventService = new SiteEventService(new MemorySiteEventStorage());

            var eventTypeId = "DCategoryA-eventIdA";
            await eventService.StoreEvent(new SiteEvent(Enums.SiteEventSeverity.Error, eventTypeId, "First event", "Some desc A")
            {
                Timestamp = DateTime.Now.AddDays(-2)
            });
            await eventService.StoreEvent(new SiteEvent(Enums.SiteEventSeverity.Error, eventTypeId, "Last event", "Some desc B")
            {
                Timestamp = DateTime.Now.AddDays(-1)
            });
            await eventService.StoreEvent(new SiteEvent(Enums.SiteEventSeverity.Error, "Some other event id", "Other event", "Some desc C")
            {
                Timestamp = DateTime.Now.AddHours(-2)
            });

            var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryA"), siteEventService: eventService);
            Assert.Contains(results, result => result.SiteEvent?.EventTypeId == eventTypeId);

            var events = await eventService.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(3, events.Count);

            var firstEvent = events.First(x => x.Title == "First event");
            Assert.False(firstEvent.Resolved);
            Assert.Null(firstEvent.ResolvedMessage);
            Assert.Equal(eventTypeId, firstEvent.EventTypeId);

            var lastEvent = events.First(x => x.Title == "Last event");
            Assert.True(lastEvent.Resolved);
            Assert.Equal("Resolved message!", lastEvent.ResolvedMessage);
            Assert.Equal(eventTypeId, lastEvent.EventTypeId);
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
        public async Task ExecuteTests_WithSuccessAndErrorOnSameEventTypeId_IgnoresSuccess()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var runner = new TestRunnerService();
            var eventService = new SiteEventService(new MemorySiteEventStorage());
            var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryE"), siteEventService: eventService);

            Assert.Equal(2, results.Count);
            Assert.Contains(results, result => result.SiteEvent?.Resolved == true);
            Assert.Contains(results, result => result.SiteEvent?.Title == "Oh no!");
            var events = await eventService.GetEvents(DateTime.MinValue, DateTime.MaxValue);

            Assert.Single(events);
            Assert.Contains(events, e => e.Title == "Oh no!" && e.Resolved == false);
        }

        [Fact]
        public async Task ExecuteTests_WithMultipleErrorsWithSameEventTypeId_MergesEventData()
        {
            var discoverer = new TestDiscoveryService()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var runner = new TestRunnerService();
            var eventService = new SiteEventService(new MemorySiteEventStorage());
            var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryF"), siteEventService: eventService);

            Assert.Equal(4, results.Count);
            Assert.True(results.All(x => x.Message.Contains("Opsie ")));
            var events = await eventService.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            
            Assert.Equal(2, events.Count);
            var siteEvent = events.First();
            var highestSeverityEvent = results.OrderByDescending(x => x.SiteEvent.Severity).First().SiteEvent;

            Assert.Equal(highestSeverityEvent.Title, siteEvent.Title);
            Assert.Equal(highestSeverityEvent.Severity, siteEvent.Severity);
            Assert.Contains("Desc Information!", siteEvent.Description);
            Assert.Contains("Desc Warning!", siteEvent.Description);
            Assert.Contains("Desc Fatal!", siteEvent.Description);
            Assert.DoesNotContain("Other Error!", siteEvent.Description);

            Output.WriteLine(siteEvent.Description);
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

        [RuntimeTestClass(Id = "TestRunnerTestsSetD", Description = "Some test set D", Name = "Dev test set D")]
        public class TestClassD
        {
            [RuntimeTest(Category = "DCategoryA")]
            public TestResult TestThatResolvesEvent()
            {
                return TestResult.CreateSuccess("Ok")
                    .SetSiteEvent(new SiteEvent("DCategoryA-eventIdA", "Resolved message!"));
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetE", Description = "Some test set E", Name = "Dev test set E", DefaultCategory = "DCategoryE")]
        public class TestClassE
        {
            private const string EventTypeId = "DCategoryE-eventIdE";

            [RuntimeTest]
            public TestResult Success()
            {
                return TestResult.CreateResolvedSiteEvent("Ok", EventTypeId, "Resolved message!");
            }

            [RuntimeTest]
            public TestResult ErrorA()
            {
                return TestResult.CreateError("Opsie!")
                    .SetSiteEvent(new SiteEvent(Enums.SiteEventSeverity.Error, EventTypeId, "Oh no!", "Desc!"));
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetF", Description = "Some test set F", Name = "Dev test set F", DefaultCategory = "DCategoryF")]
        public class TestClassF
        {
            private const string EventTypeId = "DCategoryF-eventIdF";

            [RuntimeTest]
            public TestResult ErrorA()
            {
                return TestResult.CreateError("Opsie Warning!")
                    .SetSiteEvent(new SiteEvent(Enums.SiteEventSeverity.Warning, EventTypeId, "Oh no Warning!", "Desc Warning!"));
            }

            [RuntimeTest]
            public TestResult ErrorB()
            {
                return TestResult.CreateError("Opsie Information!")
                    .SetSiteEvent(new SiteEvent(Enums.SiteEventSeverity.Information, EventTypeId, "Oh no Information!", "Desc Information!"));
            }

            [RuntimeTest]
            public TestResult ErrorC()
            {
                return TestResult.CreateError("Opsie Fatal!")
                    .SetSiteEvent(new SiteEvent(Enums.SiteEventSeverity.Fatal, EventTypeId, "Oh no Fatal!", "Desc Fatal!"));
            }

            [RuntimeTest]
            public TestResult ErrorD()
            {
                return TestResult.CreateError("Opsie Error!")
                    .SetSiteEvent(new SiteEvent(Enums.SiteEventSeverity.Error, EventTypeId + "2", "Oh no Error2!", "Other Error!"));
            }
        }
    }
}
