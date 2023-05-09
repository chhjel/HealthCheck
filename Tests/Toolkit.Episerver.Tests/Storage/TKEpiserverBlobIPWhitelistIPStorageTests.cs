using QoDL.Toolkit.Episerver.IPWhitelist;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Storage
{
    public class TKEpiserverBlobIPWhitelistIPStorageTests
    {
        [Fact]
        public async Task StoreRuleIP_Twice_ShouldCreateTwoIPs()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            TKIPWhitelistIP createIP(string value)
                => new TKIPWhitelistIP() { IP = value };

            await storage.StoreRuleIPAsync(createIP("a"));
            await storage.StoreRuleIPAsync(createIP("b"));
            storage.ForceBufferCallback();

            var items = (await storage.GetAllIPsAsync()).ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal("a", items[0].IP);
            Assert.Equal("b", items[1].IP);
        }

        [Fact]
        public async Task StoreRuleIP_UpdateExisting_ShouldUpdateExisting()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            TKIPWhitelistIP createIP(string value)
                => new TKIPWhitelistIP() { IP = value };

            var item = await storage.StoreRuleIPAsync(createIP("a"));
            item.IP = "b";
            await storage.StoreRuleIPAsync(item);
            storage.ForceBufferCallback();

            var items = (await storage.GetAllIPsAsync()).ToList();
            Assert.Single(items);
            Assert.Equal("b", items[0].IP);
        }

        private TKEpiserverBlobIPWhitelistIPStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new TKEpiserverBlobIPWhitelistIPStorage(factoryMock.Object, cache);
        }
    }
}
