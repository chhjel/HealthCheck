using EPiServer.Framework.Blobs;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Episerver.Storage;
using HealthCheck.Episerver.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCheck.Episerver.Tests.Storage
{
    public class HCEpiserverBlobDataRepeaterStreamItemStorageTests
    {
        public HCEpiserverBlobDataRepeaterStreamItemStorageTests()
        {
            // Init static serializer config
            _ = typeof(WebUI.Util.AssemblyUtil);
        }

        [Fact]
        public async Task StoreItem_WithMaxCount_RespectsIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(50)
                as StorageImplementation;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var data = new StreamItemData { SomeId = i.ToString(), AnotherThing = i % 2 == 0, SomeValue = i * 25m };
                tasks.Add(storage.StoreItemAsync(StreamItem.CreateFrom(data, data.SomeId)));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            var items = (await storage.GetItemsPagedAsync(new HCGetDataRepeaterStreamItemsFilteredRequest
                { PageIndex = 0, PageSize = int.MaxValue })).Items.ToList();
            Assert.Equal(50, items.Count);
        }

        [Fact]
        public async Task StoreItem_WithMaxAge_RespectsIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemAge(TimeSpan.FromDays(1))
                as StorageImplementation;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var data = new StreamItemData { SomeId = i.ToString(), AnotherThing = i % 2 == 0, SomeValue = i * 25m };
                var item = StreamItem.CreateFrom(data, data.SomeId);
                if (i % 2 == 0)
                {
                    item.InsertedAt = item.InsertedAt.AddDays(-10);
                }
                tasks.Add(storage.StoreItemAsync(item));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            var items = (await storage.GetItemsPagedAsync(new HCGetDataRepeaterStreamItemsFilteredRequest
            { PageIndex = 0, PageSize = int.MaxValue })).Items.ToList();
            Assert.Equal(50, items.Count);
        }

        [Fact]
        public async Task AddItemTag_TwiceOnSameItem_UpdatesIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            async Task<List<IHCDataRepeaterStreamItem>> getItemsAsync()
                => (await storage.GetItemsPagedAsync(new HCGetDataRepeaterStreamItemsFilteredRequest
                { PageIndex = 0, PageSize = int.MaxValue })).Items.ToList();

            var tasks = new List<Task>();
            StreamItem targetItem = null;
            for (int i = 0; i < 10; i++)
            {
                var data = new StreamItemData { SomeId = i.ToString(), AnotherThing = i % 2 == 0, SomeValue = i * 25m };
                var item = StreamItem.CreateFrom(data, data.SomeId);
                if (i == 8)
                {
                    targetItem = item;
                }
                tasks.Add(storage.StoreItemAsync(item));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            await storage.AddItemTagAsync(targetItem.Id, "Tag 1");
            var items = (await getItemsAsync());
            Assert.True(items.Count(x => x.Tags.Count > 0) == 1);
            Assert.True(items.Count(x => x.Id == targetItem.Id && x.Tags.Single() == "Tag 1") == 1);

            storage.ForceBufferCallback();
            await storage.AddItemTagAsync(targetItem.Id, "Tag 2");
            items = (await getItemsAsync());
            Assert.True(items.Count(x => x.Tags.Count > 0) == 1);
            Assert.True(items.Count(x => x.Id == targetItem.Id && x.Tags.Count == 2 && x.Tags.Contains("Tag 1") && x.Tags.Contains("Tag 2")) == 1);

            Assert.Equal(10, items.Count);
        }

        [Fact]
        public async Task RemoveAllItemTags_OnItemWithTags_RemovesThem()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            async Task<List<IHCDataRepeaterStreamItem>> getItemsAsync()
                => (await storage.GetItemsPagedAsync(new HCGetDataRepeaterStreamItemsFilteredRequest
                { PageIndex = 0, PageSize = int.MaxValue })).Items.ToList();

            var tasks = new List<Task>();
            StreamItem targetItem = null;
            for (int i = 0; i < 10; i++)
            {
                var data = new StreamItemData { SomeId = i.ToString(), AnotherThing = i % 2 == 0, SomeValue = i * 25m };
                var item = StreamItem.CreateFrom(data, data.SomeId);
                if (i == 8)
                {
                    targetItem = item;
                }
                tasks.Add(storage.StoreItemAsync(item));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            await storage.AddItemTagAsync(targetItem.Id, "Tag 1");
            var items = (await getItemsAsync());
            Assert.True(items.Count(x => x.Tags.Count > 0) == 1);
            Assert.True(items.Count(x => x.Id == targetItem.Id && x.Tags.Single() == "Tag 1") == 1);

            storage.ForceBufferCallback();
            await storage.AddItemTagAsync(targetItem.Id, "Tag 2");
            await storage.RemoveAllItemTagsAsync(targetItem.Id);
            items = (await getItemsAsync());
            Assert.True(items.Count(x => x.Tags.Count > 0) == 0);

            Assert.Equal(10, items.Count);
        }

        [Fact]
        public async Task RemoveItemTag_OnItemWithTags_RemovesThem()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            async Task<List<IHCDataRepeaterStreamItem>> getItemsAsync()
                => (await storage.GetItemsPagedAsync(new HCGetDataRepeaterStreamItemsFilteredRequest
                { PageIndex = 0, PageSize = int.MaxValue })).Items.ToList();

            var tasks = new List<Task>();
            StreamItem targetItem = null;
            for (int i = 0; i < 10; i++)
            {
                var data = new StreamItemData { SomeId = i.ToString(), AnotherThing = i % 2 == 0, SomeValue = i * 25m };
                var item = StreamItem.CreateFrom(data, data.SomeId);
                if (i == 8)
                {
                    targetItem = item;
                }
                tasks.Add(storage.StoreItemAsync(item));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            await storage.AddItemTagAsync(targetItem.Id, "Tag 1");
            var items = (await getItemsAsync());
            Assert.True(items.Count(x => x.Tags.Count > 0) == 1);
            Assert.True(items.Count(x => x.Id == targetItem.Id && x.Tags.Single() == "Tag 1") == 1);

            storage.ForceBufferCallback();
            await storage.AddItemTagAsync(targetItem.Id, "Tag 2");
            await storage.RemoveItemTagAsync(targetItem.Id, "Tag 1");
            items = (await getItemsAsync());
            Assert.True(items.Count(x => x.Tags.Count > 0) == 1);
            Assert.True(items.Count(x => x.Id == targetItem.Id && x.Tags.Single() == "Tag 2") == 1);

            Assert.Equal(10, items.Count);
        }

        [Fact]
        public async Task DeleteItem_WithExistingItem_RemovesIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            async Task<List<IHCDataRepeaterStreamItem>> getItemsAsync()
                => (await storage.GetItemsPagedAsync(new HCGetDataRepeaterStreamItemsFilteredRequest
                { PageIndex = 0, PageSize = int.MaxValue })).Items.ToList();

            var tasks = new List<Task>();
            StreamItem targetItem1 = null;
            StreamItem targetItem2 = null;
            for (int i = 0; i < 10; i++)
            {
                var data = new StreamItemData { SomeId = i.ToString(), AnotherThing = i % 2 == 0, SomeValue = i * 25m };
                var item = StreamItem.CreateFrom(data, data.SomeId);
                if (i == 6) targetItem1 = item;
                else if (i == 8) targetItem2 = item;
                tasks.Add(storage.StoreItemAsync(item));
            }
            await Task.WhenAll(tasks);

            await storage.DeleteItemAsync(targetItem1.Id);

            var items = (await getItemsAsync());
            Assert.Equal(9, items.Count);
            Assert.True(items.Count(x => x.Id == targetItem1.Id) == 0);

            storage.ForceBufferCallback();
            
            await storage.DeleteItemAsync(targetItem2.Id);

            items = (await getItemsAsync());
            Assert.Equal(8, items.Count);
            Assert.True(items.Count(x => x.Id == targetItem2.Id) == 0);
        }

        private StorageImplementation CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new StorageImplementation(factoryMock.Object, cache);
        }

        public class StreamItemData
        {
            public string SomeId { get; set; }
            public decimal SomeValue { get; set; }
            public bool AnotherThing { get; set; }
        }
        public class StreamItem : HCDefaultDataRepeaterStreamItem<StreamItemData, StreamItem> {}
        public class StorageImplementation : HCEpiserverBlobDataRepeaterStreamItemStorage<StreamItem>
        {
            protected override Guid ContainerId => Guid.Parse("c0254918-bb23-4ebb-9890-062ed6a11aaa");

            public StorageImplementation(IBlobFactory blobFactory, Core.Abstractions.IHCCache cache) : base(blobFactory, cache) {}
        }
    }
}