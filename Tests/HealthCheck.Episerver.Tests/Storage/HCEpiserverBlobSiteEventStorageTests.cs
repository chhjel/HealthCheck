using HealthCheck.Core.Modules.SiteEvents.Enums;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Modules.SiteEvents.Services;
using HealthCheck.Episerver.Storage;
using HealthCheck.Episerver.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCheck.Episerver.Tests.Storage
{
    public class HCEpiserverBlobSiteEventStorageTests
    {
        [Fact]
        public async Task StoreEvent_WithMaxCount_RespectsIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(50)
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id)
                => new SiteEvent { EventTypeId = "type_" + id, Title = $"Title#{id}", Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(service.StoreEvent(createEvent(i)));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(50, items.Count);
        }

        [Fact]
        public async Task StoreEvent_WithMaxAge_RespectsIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemAge(TimeSpan.FromDays(1))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id)
                => new SiteEvent { EventTypeId = "type_" + id, Title = $"Title#{id}", Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var e = createEvent(i);
                if (i % 2 == 0)
                {
                    e.Timestamp = e.Timestamp.AddDays(-10);
                }
                tasks.Add(service.StoreEvent(e));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(50, items.Count);
        }

        [Fact]
        public async Task StoreEvent_ToUpdateUsingService_ShouldNotCreateNew()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id)
                => new SiteEvent { EventTypeId = "type_x", Title = $"Title#{id}", Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(service.StoreEvent(createEvent(i)));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.True(items.TrueForAll(x => x.Id != Guid.Empty), "All items should have generated ids");
            Assert.True(items.GroupBy(x => x.Id).All(x => x.Count() == 1), "All items should have unique ids");
            Assert.Single(items);

            storage.ForceBufferCallback();
            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
        }

        [Fact]
        public async Task StoreEvent_ToUpdateUsingService_UpdatesCorrectItem()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id, string name)
                => new SiteEvent { EventTypeId = "type_" + id, Title = name, Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            tasks.Add(service.StoreEvent(createEvent(0, "A")));
            tasks.Add(service.StoreEvent(createEvent(1, "B")));
            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            items[0].Title = "A";
            items[1].Title = "B";

            storage.ForceBufferCallback();
            tasks.Add(service.StoreEvent(createEvent(0, "A")));
            tasks.Add(service.StoreEvent(createEvent(1, "B")));

            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            items[0].Title = "Av2";
            items[1].Title = "Bv2";

            storage.ForceBufferCallback();

            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            items[0].Title = "Av2";
            items[1].Title = "Bv2";
        }

        [Fact]
        public async Task StoreEvent_WithUpdateOutsideMergeWindow_RespectsMergeThreshold()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id, string name, DateTimeOffset timestamp)
                => new SiteEvent { EventTypeId = "type_" + id, Title = name, Description = $"Desc#{id}", Timestamp = timestamp, Duration = 1 };

            var service = new SiteEventService(storage);

            await service.StoreEvent(createEvent(1, "Event Old", DateTimeOffset.Now.AddMinutes(-(service.DefaultMergeOptions.MaxMinutesSinceLastEventEnd+2))));
            storage.ForceBufferCallback();

            await service.StoreEvent(createEvent(1, "Event New", DateTimeOffset.Now));
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            Assert.Equal("Event Old", items[0].Title);
            Assert.Equal("Event New", items[1].Title);
        }

        [Fact]
        public async Task StoreEvent_WithUpdateWithinMergeWindow_RespectsMergeThreshold()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id, string name, DateTimeOffset timestamp)
                => new SiteEvent { EventTypeId = "type_" + id, Title = name, Description = $"Desc#{id}", Timestamp = timestamp, Duration = 1 };

            var service = new SiteEventService(storage);

            await service.StoreEvent(createEvent(1, "Event Old", DateTimeOffset.Now - TimeSpan.FromMinutes(service.DefaultMergeOptions.MaxMinutesSinceLastEventEnd / 2)));
            storage.ForceBufferCallback();

            await service.StoreEvent(createEvent(1, "Event New", DateTimeOffset.Now));
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
            Assert.Equal("Event New", items[0].Title);
        }

        [Fact]
        public async Task StoreEvent_WhenBuffered_ItemsShouldHaveUniqueIds()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var eventX = new SiteEvent
                {
                    EventTypeId = "typeId_" + i,
                    Title = $"Title#{i}",
                    Description = $"Desc#{i}",
                };
                tasks.Add(storage.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.True(items.TrueForAll(x => x.Id != Guid.Empty), "All items should have generated ids");
            Assert.True(items.GroupBy(x => x.Id).All(x => x.Count() == 1), "All items should have unique ids");
        }

        [Fact]
        public async Task StoreEvent_WithLessThanMax_ShouldBeAvailableAtOnce()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeId_" + i, "TitleB", "DescriptionB");
                tasks.Add(storage.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(100, items.Count); // Should be 100 items stored
            Assert.Equal(1, storage.LoadCounter); // Should get only once
            Assert.Equal(0, storage.SaveCounter); // Store count should be inserts / max buffer size = 0
        }

        [Fact]
        public async Task StoreEvent_WithMaxCount_ShouldNotOverflow()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;

            var tasks = new List<Task>();
            for (int i = 0; i < 5000; i++)
            {
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeId_"+i, "TitleB", "DescriptionB");
                tasks.Add(storage.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(1000, items.Count); // Should be 1000 items stored
            Assert.Equal(1, storage.LoadCounter); // Should get only once
            Assert.Equal(10, storage.SaveCounter); // Store count should be inserts / max buffer size = 0
        }

        [Fact]
        public async Task StoreEvent_WithManyMergedEvents_ShouldStoreSingle()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
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
        public async Task MarkEventAsResolved_WithEventFromYesterday_ShouldResolveEvent()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent e = new SiteEvent { EventTypeId = "id", Title = "Title", Description = $"Desc", Timestamp = DateTimeOffset.Now.AddDays(-1), Duration = 1 };

            var service = new SiteEventService(storage);

            await service.StoreEvent(e);
            storage.ForceBufferCallback();

            await service.MarkEventAsResolved(e.EventTypeId, "Resolved!");
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
            Assert.Equal(e.Id, items[0].Id);
            Assert.Equal(e.EventTypeId, items[0].EventTypeId);
            Assert.Equal("Resolved!", items[0].ResolvedMessage);
            Assert.True(items[0].Resolved);
        }

        [Fact]
        public async Task MarkEventAsResolved_WithEventFromYesterdayAndResolvedEarlier_ShouldResolveEvent()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as HCEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var service = new SiteEventService(storage);

            var earlyEvent = new SiteEvent { EventTypeId = "id1", Title = "Title1", Description = $"Desc1", Timestamp = DateTimeOffset.Now.AddDays(-2), Duration = 1,
                Resolved = true, ResolvedAt = DateTimeOffset.Now.AddDays(-1.5), ResolvedMessage = "Resolved1"
            };
            await service.StoreEvent(earlyEvent);
            storage.ForceBufferCallback();

            var e = new SiteEvent { EventTypeId = "id2", Title = "Title2", Description = $"Desc2", Timestamp = DateTimeOffset.Now.AddDays(-1), Duration = 1 };
            await service.StoreEvent(e);
            storage.ForceBufferCallback();

            await service.MarkEventAsResolved(e.EventTypeId, "Resolved2");
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            Assert.Equal(earlyEvent.Id, items[0].Id);
            Assert.Equal(e.Id, items[1].Id);
            Assert.Equal("id1", items[0].EventTypeId);
            Assert.Equal("id2", items[1].EventTypeId);
            Assert.Equal("Resolved1", items[0].ResolvedMessage);
            Assert.Equal("Resolved2", items[1].ResolvedMessage);
            Assert.True(items[0].Resolved);
            Assert.True(items[1].Resolved);
        }

        private HCEpiserverBlobSiteEventStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new HCEpiserverBlobSiteEventStorage(factoryMock.Object, cache);
        }
    }
}
