using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Episerver.Storage;
using HealthCheck.Episerver.Tests.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCheck.Episerver.Tests.Storage
{
    public class HCEpiserverBlobStoredDataflowStreamTests
    {
        [Fact]
        public async Task InsertEntry_WhenBuffered_ShouldBeAvailableAtOnce()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);

            for (int i = 0; i < 100; i++)
            {
                var item = new DummyItem
                {
                    Id = Guid.NewGuid().ToString()
                };
                storage.InsertEntry(item);
            }

            var items = (await storage.GetLatestStreamEntriesAsync(new DataflowStreamFilter { Take = 500 }))
                .OfType<DummyItem>()
                .ToList();
            Assert.Equal(100, items.Count); // Should be 100 items stored
        }

        private DummyStream CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new DummyStream(factoryMock.Object, cache, Guid.NewGuid(), x => x.Id);
        }
        public class DummyStream : HCEpiserverBlobStoredDataflowStream<DummyEnum, DummyItem, string>
        {
            public override string Name => "Dummy stream";
            public override string Description => "Desc";
            public override string GroupName => "Group";
            public DummyStream(IBlobFactory blobFactory, IHCCache cache, Guid containerId, Func<DummyItem, string> idSelector)
                : base(blobFactory, cache, containerId, idSelector)
            {
                BlobStorage.MaxBufferSize = 500;
                BlobStorage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);
            }
        }
        public class DummyItem : IDataflowEntryWithInsertionTime
        {
            public string Id { get; set; }
            public DateTimeOffset? InsertionTime { get; set; }
        }
        public enum DummyEnum { }
    }
}
