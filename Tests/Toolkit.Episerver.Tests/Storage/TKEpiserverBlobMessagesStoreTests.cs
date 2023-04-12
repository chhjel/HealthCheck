using QoDL.Toolkit.Core.Modules.Messages.Models;
using QoDL.Toolkit.Episerver.Storage;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using System;
using System.Linq;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Storage
{
    public class TKEpiserverBlobMessagesStoreTests
    {
        [Fact]
        public void StoreMessage_WhenBuffered_ShouldBeAvailableAtOnce()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);

            for (int i = 0; i < 100; i++)
            {
                var msg = new TKDefaultMessageItem($"Some summary here #{i}", $"{i}345678", $"841244{i}", $"Some test message #{i} here etc etc.", false);
                storage.StoreMessage("sms", msg);
            }

            var items = storage.GetLatestMessages("sms", 1000, 0).Data.ToList();
            Assert.Equal(100, items.Count); // Should be 100 items stored
        }

        [Fact]
        public void StoreMessage_WhenBuffered_ItemsShouldHaveUniqueIds()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);

            for (int i = 0; i < 100; i++)
            {
                var msg = new TKDefaultMessageItem($"Some summary here #{i}", $"{i}345678", $"841244{i}", $"Some test message #{i} here etc etc.", false);
                storage.StoreMessage("sms", msg);
            }

            var items = storage.GetLatestMessages("sms", 1000, 0).Data.ToList();
            Assert.True(items.TrueForAll(x => !string.IsNullOrWhiteSpace(x.Id)), "All items should have generated ids");
            Assert.True(items.GroupBy(x => x.Id).All(x => x.Count() == 1), "All items should have unique ids");
        }

        private TKEpiserverBlobMessagesStore<TKDefaultMessageItem> CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new TKEpiserverBlobMessagesStore<TKDefaultMessageItem>(factoryMock.Object, cache);
        }
    }
}
