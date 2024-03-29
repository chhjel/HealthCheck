using QoDL.Toolkit.Core.Modules.SiteEvents.Enums;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Modules.SiteEvents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Modules.SiteEvents;

public class SiteEventServiceTests
{
    public ITestOutputHelper Output { get; }

    public SiteEventServiceTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public async Task MarkLatestEventAsResolved_WithoutMatchingEvent_ShouldDoNothing()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA");
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdB", "TitleB", "DescriptionB");
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        await service.MarkLatestEventAsResolved("typeIdC", "Resolved!");
        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Empty(items.Where(x => x.Resolved));
    }

    [Fact]
    public async Task MarkLatestEventAsResolved_WithoutMultipleMatchingEvents_ShouldMarkLast()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdX", "TitleA", "DescriptionA");
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdX", "TitleB", "DescriptionB");
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        await service.MarkLatestEventAsResolved("typeIdX", "Resolved!");
        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items.Where(x => x.Resolved && x.ResolvedMessage == "Resolved!" && x.ResolvedAt != null));
    }

    [Fact]
    public async Task StoreEvent_WithLoadsOfMatchingEvents_ShouldResultInSingle()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var tasks = new List<Task>();
        for (int i = 0; i < 5000; i++)
        {
            var eventX = new SiteEvent(SiteEventSeverity.Error, "typeIdX", "TitleB", "DescriptionB");
            tasks.Add(service.StoreEvent(eventX));
        }

        await Task.WhenAll(tasks);
        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);
    }

    [Fact]
    public async Task StoreEvent_WithoutOverlappingEvents_ShouldAddNew()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-30)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task StoreEvent_WithOverlappingEvents_ShouldExtendPrevious()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-3)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.Equal(5, item.Duration);
    }

    [Fact]
    public async Task StoreEvent_WithOverlappingEventsWithDifferentIds_ShouldNotNotMerge()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-3)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdB", "TitleB", "DescriptionB", duration: 10);
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task StoreEvent_WithOverlappingEvents_ShouldNotExtendPastCurrentTime()
    {
        var storage = new MemorySiteEventStorage();
        var service = new SiteEventService(storage);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-10)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 30);
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.Equal(10, item.Duration);
    }

    [Fact]
    public async Task StoreEvent_5MinutesSinceLastAndIncreasedThreshold_ShouldExtendPrevious()
    {
        var storage = new MemorySiteEventStorage();
        var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: null);
        var service = new SiteEventService(storage)
            .SetDefaultMergeOptions(defaultMergeOptions);

        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 10)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-15)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);

        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.Equal(15, item.Duration);
    }

    [Fact]
    public async Task StoreEvent_WithoutEventTypeIds_ShouldNotExtendPrevious()
    {
        var storage = new MemorySiteEventStorage();
        var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: null);
        var service = new SiteEventService(storage)
            .SetDefaultMergeOptions(defaultMergeOptions);

        var eventA = new SiteEvent(SiteEventSeverity.Error, null, "TitleA", "DescriptionA", duration: 10)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-15)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, null, "TitleB", "DescriptionB", duration: 5);

        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task StoreEvent_LastMultiplierShouldCreateOverlap_ShouldExtendPrevious()
    {
        var storage = new MemorySiteEventStorage();
        var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: 2f);
        var service = new SiteEventService(storage)
            .SetDefaultMergeOptions(defaultMergeOptions);

        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 10)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-25)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);

        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.Equal(25, item.Duration);
    }

    [Fact]
    public async Task StoreEvent_ErrorWithPreviouslyResolvedEvent_ShouldExtendPreviousAndSetUnresolved()
    {
        var storage = new MemorySiteEventStorage();
        var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: null);
        var service = new SiteEventService(storage)
            .SetDefaultMergeOptions(defaultMergeOptions);

        var previouslyResolvedEvent = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 10)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-15),
            ResolvedAt = DateTimeOffset.Now.AddMinutes(-14),
            Resolved = true,
            ResolvedMessage = "Resolved"
        };
        await service.StoreEvent(previouslyResolvedEvent);

        var newUnresolvedEvent = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
        await service.StoreEvent(newUnresolvedEvent);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.False(item.Resolved);
        Assert.Equal(15, item.Duration);
    }

    [Fact]
    public async Task StoreEvent_ResolvedWithPreviouslyUnresolvedEvent_ShouldSetPreviousAsResolved()
    {
        var storage = new MemorySiteEventStorage();
        var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: null);
        var service = new SiteEventService(storage)
            .SetDefaultMergeOptions(defaultMergeOptions);

        var previouslyUnresolvedEvent = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 10)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-15)
        };
        await service.StoreEvent(previouslyUnresolvedEvent);

        var newResolvedEvent = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5)
        {
            ResolvedAt = DateTimeOffset.Now,
            Resolved = true,
            ResolvedMessage = "Resolved"
        };
        await service.StoreEvent(newResolvedEvent);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.True(item.Resolved);
        Assert.Equal(15, item.Duration);
    }

    [Fact]
    public async Task StoreEvent_CustomMergeLocic_ShouldApply()
    {
        var NewDescription = "New description!";

        var storage = new MemorySiteEventStorage();
        var defaultMergeOptions = new SiteEventMergeOptions(
            allowEventMerge: true,
            maxMinutesSinceLastEventEnd: 15,
            lastEventDurationMultiplier: 2f,
            eventMerger: (oldEvent, newEvent) =>
            {
                oldEvent.Description = NewDescription;
                oldEvent.Duration += newEvent.Duration;
            }
        );
        var service = new SiteEventService(storage)
            .SetDefaultMergeOptions(defaultMergeOptions);
        var eventA = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
        {
            Timestamp = DateTimeOffset.Now.AddMinutes(-3)
        };
        var eventB = new SiteEvent(SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
        await service.StoreEvent(eventA);
        await service.StoreEvent(eventB);

        var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
        Assert.Single(items);

        var item = items.Single();
        Assert.Equal(10, item.Duration);
        Assert.Equal(item.Description, NewDescription);
    }

    [Fact]
    public void DefaultMergeLogic_Merges_AccordingToDescription()
    {
        var eventTypeId = "merge-test-event";
        var existingEvent = new SiteEvent(SiteEventSeverity.Error, eventTypeId, "Old title", "Old desc", duration: 5, developerDetails: "Old details")
            .AddRelatedLink("old title", "old url");

        var newSeverity = SiteEventSeverity.Information;
        var newTitle = "New title";
        var newDesc = "New desc";
        var newDetails = "New details";
        var newEvent = new SiteEvent(newSeverity, eventTypeId, newTitle, newDesc, duration: 5, developerDetails: newDetails)
            .AddRelatedLink("new title", "new url");

        SiteEventMergeOptions.DefaultMergeLogic(existingEvent, newEvent);
        Assert.Equal(newSeverity, existingEvent.Severity);
        Assert.Equal(newTitle, existingEvent.Title);
        Assert.Equal(newDesc, existingEvent.Description);
        Assert.Equal(newDetails, existingEvent.DeveloperDetails);

        Assert.Equal(2, existingEvent.RelatedLinks.Count);
        var firstLink = existingEvent.RelatedLinks[0];
        Assert.Equal("old title", firstLink.Text);
        Assert.Equal("old url", firstLink.Url);
        var secondLink = existingEvent.RelatedLinks[1];
        Assert.Equal("new title", secondLink.Text);
        Assert.Equal("new url", secondLink.Url);

        SiteEventMergeOptions.DefaultMergeLogic(existingEvent, newEvent);
        Assert.Equal(2, existingEvent.RelatedLinks.Count);
    }
}
