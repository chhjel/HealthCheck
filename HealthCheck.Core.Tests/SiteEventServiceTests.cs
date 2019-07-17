using HealthCheck.Core.Entities;
using HealthCheck.Core.Services.Models;
using HealthCheck.Core.Services.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.Core.Services
{
    public class SiteEventServiceTests
    {
        public ITestOutputHelper Output { get; }

        public SiteEventServiceTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public async Task StoreEvent_WithoutOverlappingEvents_ShouldAddNew()
        {
            var storage = new MemorySiteEventStorage();
            var service = new SiteEventService(storage);
            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
            {
                Timestamp = DateTime.Now.AddMinutes(-30)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task StoreEvent_WithOverlappingEvents_ShouldExtendPrevious()
        {
            var storage = new MemorySiteEventStorage();
            var service = new SiteEventService(storage);
            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
            {
                 Timestamp = DateTime.Now.AddMinutes(-3)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Single(items);

            var item = items.Single();
            Assert.Equal(5, item.Duration);
        }

        [Fact]
        public async Task StoreEvent_WithOverlappingEventsWithDifferentIds_ShouldNotNotMerge()
        {
            var storage = new MemorySiteEventStorage();
            var service = new SiteEventService(storage);
            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
            {
                Timestamp = DateTime.Now.AddMinutes(-3)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdB", "TitleB", "DescriptionB", duration: 10);
            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task StoreEvent_WithOverlappingEvents_ShouldNotExtendPastCurrentTime()
        {
            var storage = new MemorySiteEventStorage();
            var service = new SiteEventService(storage);
            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
            {
                Timestamp = DateTime.Now.AddMinutes(-10)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 30);
            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Single(items);

            var item = items.Single();
            Assert.Equal(10, item.Duration);
        }

        [Fact]
        public async Task StoreEvent_5MinutesSinceLastAndIncreasedThreshold_ShouldExtendPrevious()
        {
            var storage = new MemorySiteEventStorage();
            var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: null);
            var service = new SiteEventService(storage, defaultMergeOptions);

            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 10)
            {
                Timestamp = DateTime.Now.AddMinutes(-15)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);

            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Single(items);

            var item = items.Single();
            Assert.Equal(15, item.Duration);
        }

        [Fact]
        public async Task StoreEvent_LastMultiplierShouldCreateOverlap_ShouldExtendPrevious()
        {
            var storage = new MemorySiteEventStorage();
            var defaultMergeOptions = new SiteEventMergeOptions(allowEventMerge: true, maxMinutesSinceLastEventEnd: 10, lastEventDurationMultiplier: 2f);
            var service = new SiteEventService(storage, defaultMergeOptions);

            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 10)
            {
                Timestamp = DateTime.Now.AddMinutes(-25)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);

            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Single(items);

            var item = items.Single();
            Assert.Equal(25, item.Duration);
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
                eventMerger: (oldEvent, newEvent) => {
                    oldEvent.Description = NewDescription;
                    oldEvent.Duration += newEvent.Duration;
                }
            );
            var service = new SiteEventService(storage, defaultMergeOptions);
            var eventA = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleA", "DescriptionA", duration: 5)
            {
                Timestamp = DateTime.Now.AddMinutes(-3)
            };
            var eventB = new SiteEvent(Enums.SiteEventSeverity.Error, "typeIdA", "TitleB", "DescriptionB", duration: 5);
            await service.StoreEvent(eventA);
            await service.StoreEvent(eventB);

            var items = await storage.GetEvents(DateTime.MinValue, DateTime.MaxValue);
            Assert.Single(items);

            var item = items.Single();
            Assert.Equal(10, item.Duration);
            Assert.Equal(item.Description, NewDescription);
        }
    }
}
