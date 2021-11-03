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
        public async Task UpdateEvent_UsingService_ShouldNotCreateNew()
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
        public async Task UpdateEvent_UsingService_UpdatesCorrectItem()
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

        private HCEpiserverBlobSiteEventStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new HCEpiserverBlobSiteEventStorage(factoryMock.Object, cache);
        }
    }
}
