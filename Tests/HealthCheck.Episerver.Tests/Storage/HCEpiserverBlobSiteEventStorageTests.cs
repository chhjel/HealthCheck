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
        public async Task StoreEvent_WhenBuffered_ItemsShouldHaveUniqueIds()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var eventX = new SiteEvent {
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
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeIdX", "TitleB", "DescriptionB");
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
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeIdX", "TitleB", "DescriptionB");
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
