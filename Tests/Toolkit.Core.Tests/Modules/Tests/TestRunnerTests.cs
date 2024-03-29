using QoDL.Toolkit.Core.Modules.AuditLog.Services;
using QoDL.Toolkit.Core.Modules.SiteEvents.Enums;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Modules.SiteEvents.Services;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Modules.Tests.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Modules.Tests;

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
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("CategoryA"));
        Assert.Single(results);
        Assert.Contains("CategoryA", results.Single().Test.Categories);
    }

    [Fact]
    public async Task ExecuteTests_WithEventService_CanResolveLastMatchingEvent()
    {
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var eventService = new SiteEventService(new MemorySiteEventStorage());

        var eventTypeId = "DCategoryA-eventIdA";
        await eventService.StoreEvent(new SiteEvent(SiteEventSeverity.Error, eventTypeId, "First event", "Some desc A")
        {
            Timestamp = DateTimeOffset.Now.AddDays(-2)
        });
        await eventService.StoreEvent(new SiteEvent(SiteEventSeverity.Error, eventTypeId, "Last event", "Some desc B")
        {
            Timestamp = DateTimeOffset.Now.AddDays(-1)
        });
        await eventService.StoreEvent(new SiteEvent(SiteEventSeverity.Error, "Some other event id", "Other event", "Some desc C")
        {
            Timestamp = DateTimeOffset.Now.AddHours(-2)
        });

        var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryA"), siteEventService: eventService);
        Assert.Contains(results, result => result.SiteEvent?.EventTypeId == eventTypeId);

        var events = await eventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
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
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var eventService = new SiteEventService(new MemorySiteEventStorage());
        var results = await runner.ExecuteTests(discoverer, (test) => true, siteEventService: eventService);
        Assert.Contains(results, result => result.SiteEvent?.Title == "EventA");

        var events = await eventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Contains(events, e => e.Title == "EventA");
    }

    [Fact]
    public async Task ExecuteTests_WithSuccessAndErrorOnSameEventTypeIdAndExisting_IgnoresSuccess()
    {
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var eventService = new SiteEventService(new MemorySiteEventStorage());

        var previouslyUnresolvedEvent = new SiteEvent(SiteEventSeverity.Error, "DCategoryE-eventIdE", "Titleasd", "Descriptionasd", duration: 5)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-15),
            Resolved = true,
            ResolvedAt = DateTimeOffset.Now.AddMinutes(-10)
        };
        await eventService.StoreEvent(previouslyUnresolvedEvent);

        var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryE"), siteEventService: eventService);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, result => result.SiteEvent?.Resolved == true);
        Assert.Contains(results, result => result.SiteEvent?.Title == "Oh no!");
        var events = await eventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.Single(events);
        Assert.Contains(events, e => e.Title == "Oh no!" && !e.Resolved);
    }

    [Fact]
    public async Task ExecuteTests_WithSuccessAndErrorOnSameEventTypeId_IgnoresSuccess()
    {
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var eventService = new SiteEventService(new MemorySiteEventStorage());

        var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryE"), siteEventService: eventService);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, result => result.SiteEvent?.Resolved == true);
        Assert.Contains(results, result => result.SiteEvent?.Title == "Oh no!");
        var events = await eventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.Single(events);
        Assert.Contains(events, e => e.Title == "Oh no!" && !e.Resolved);
    }

    [Fact]
    public async Task ExecuteTests_WithSuccessAndWarningOnSameEventTypeId_IgnoresSuccess()
    {
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var eventService = new SiteEventService(new MemorySiteEventStorage());
        var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryG"), siteEventService: eventService);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, result => result.SiteEvent?.Resolved == true);
        Assert.Contains(results, result => result.SiteEvent?.Title == "Oh no!");
        var events = await eventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.Single(events);
        Assert.Contains(events, e => e.Title == "Oh no!" && !e.Resolved);
    }

    [Fact]
    public async Task ExecuteTests_WithMultipleErrorsWithSameEventTypeId_MergesEventData()
    {
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var eventService = new SiteEventService(new MemorySiteEventStorage());
        var results = await runner.ExecuteTests(discoverer, (test) => test.Categories.Contains("DCategoryF"), siteEventService: eventService);

        Assert.Equal(4, results.Count);
        Assert.True(results.All(x => x.Message.Contains("Opsie ")));
        var events = await eventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

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
        var discoverer = CreateTestDiscoveryService();
        var runner = new TestRunnerService();
        var auditService = new MemoryAuditEventStorage();
        var results = await runner.ExecuteTests(discoverer, (test) => true, onAuditEvent: (e) => auditService.StoreEvent(e));
        Assert.Contains(results, result => result.SiteEvent?.Title == "EventA");

        var events = await auditService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Contains(events, e => e.Subject == "TestMethodA");
    }

    [Fact]
    public async Task ExecuteTests_WithAFewTests_ReturnsResults()
    {
        var discoverer = CreateTestDiscoveryService();
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
        var discoverer = CreateTestDiscoveryService();
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
        var discoverer = CreateTestDiscoveryService();
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
        var discoverer = CreateTestDiscoveryService();
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
        var discoverer = CreateTestDiscoveryService();
        var testClasses = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestRunnerTestsSetC")
            .ToList();

        var runner = new TestRunnerService();
        var results = await runner.ExecuteTests(testClasses, (x) => x.Name == "Test Without Default Values");
        Assert.Single(results);

        var data = results[0].Tag as object[];
        Assert.Equal(default, data[0] as string);
        Assert.Equal(default, (DateTimeOffset)data[1]);
    }

    [Fact]
    public void ExecuteTest_WithCancellationToken_ShouldBeValidTest()
    {
        TestDiscoveryService discoverer = CreateTestDiscoveryService();
        var test = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestClass_Cancellable")
            .SelectMany(x => x.Tests)
            .FirstOrDefault(x => x.Name == "CancellableTest1");

        Assert.NotNull(test);
    }

    [Fact]
    public async Task ExecuteTest_WithAlreadyRunningCancellableTest_ShouldReturnWarning()
    {
        var discoverer = CreateTestDiscoveryService();
        var test = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestClass_Cancellable")
            .SelectMany(x => x.Tests)
            .FirstOrDefault(x => x.Name == "CancellableTest1");

        var runner = new TestRunnerService();
        var resultTask1 = runner.ExecuteTest(test, new object[0], allowDefaultValues: true);
        var resultTask2 = runner.ExecuteTest(test, new object[0], allowDefaultValues: true);

        await Task.Delay(40);
        var runner2 = new TestRunnerService();
        runner2.RequestTestCancellation(test.Id);

        var results = new TestResult[] {
            await resultTask1,
            await resultTask2
        };

        Assert.Contains(results, x => x.Status == TestResultStatus.Warning);
        Assert.Contains(results, x => x.Message == "Test is already running and must be cancelled before it can run again.");
        Assert.Contains(results, x => x.Message == "Task was cancelled.");
    }

    [Fact]
    public async Task RequestTestCancellation_WithCancellationToken_ShouldBeCancellable()
    {
        var discoverer = CreateTestDiscoveryService();
        var test = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestClass_Cancellable")
            .SelectMany(x => x.Tests)
            .FirstOrDefault(x => x.Name == "CancellableTest1");

        var runner = new TestRunnerService();
        var resultTask = runner.ExecuteTest(test, new object[0], allowDefaultValues: true);

        var runner2 = new TestRunnerService();
        runner2.RequestTestCancellation(test.Id);

        var result = await resultTask;

        Output.WriteLine(result.Message);
        Assert.Equal("Task was cancelled.", result.Message);
        Assert.Equal(TestResultStatus.Warning, result.Status);
    }

    [Fact]
    public async Task ExecuteTest_WithCancellableTestWithoutParams_ShouldRunWhenNotCancelled()
    {
        var discoverer = CreateTestDiscoveryService();
        var test = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestClass_Cancellable")
            .SelectMany(x => x.Tests)
            .FirstOrDefault(x => x.Name == "CancellableTest2");

        var runner = new TestRunnerService();
        var result = await runner.ExecuteTest(test, new object[0], allowDefaultValues: true);
        Assert.Equal("Completed!", result.Message);
    }

    [Fact]
    public async Task ExecuteTest_WithCancellableTestWithParams_ShouldRunWhenNotCancelled()
    {
        var discoverer = CreateTestDiscoveryService();
        var test = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestClass_Cancellable")
            .SelectMany(x => x.Tests)
            .FirstOrDefault(x => x.Name == "CancellableTest3");

        var runner = new TestRunnerService();
        var result = await runner.ExecuteTest(test, new object[] { "input here" }, allowDefaultValues: true);
        Assert.Equal("Completed! input here", result.Message);
    }

    [Fact]
    public async Task ExecuteTest_WithIoCErrors_ShouldGiveExplainatoryException()
    {
        var discoverer = CreateTestDiscoveryService();
        var test = discoverer.DiscoverTestDefinitions()
            .Where(x => x.Id == "TestClass_IoCExceptionA")
            .SelectMany(x => x.Tests)
            .FirstOrDefault();

        var runner = new TestRunnerService();
        var result = await runner.ExecuteTest(test, new object[] { }, allowDefaultValues: true);
        Output.WriteLine(result.Message);
        Assert.Contains("Could not create an instance of the type '", result.Message);
    }

    private TestDiscoveryService CreateTestDiscoveryService()
    {
        return new TestDiscoveryService()
        {
            AssembliesContainingTests = new[] { GetType().Assembly }
        };
    }
}
