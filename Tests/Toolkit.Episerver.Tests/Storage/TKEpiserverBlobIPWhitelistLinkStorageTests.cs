using QoDL.Toolkit.Episerver.IPWhitelist;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Storage
{
    public class TKEpiserverBlobIPWhitelistLinkStorageTests
    {
        [Fact]
        public async Task StoreRuleLink_Twice_ShouldCreateTwoRules()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var ruleId = Guid.NewGuid();
            TKIPWhitelistLink createLink(string name)
                => new TKIPWhitelistLink() { Name = name, RuleId = ruleId };

            await storage.StoreRuleLinkAsync(createLink("a"));
            await storage.StoreRuleLinkAsync(createLink("b"));
            storage.ForceBufferCallback();

            var items = (await storage.GetRuleLinksAsync(ruleId)).ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal("a", items[0].Name);
            Assert.Equal("b", items[1].Name);
        }

        [Fact]
        public async Task StoreRuleLink_UpdateExisting_ShouldUpdateExisting()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var ruleId = Guid.NewGuid();
            TKIPWhitelistLink createLink(string name)
                => new TKIPWhitelistLink() { Name = name, RuleId = ruleId };

            var item = await storage.StoreRuleLinkAsync(createLink("a"));
            item.Name = "b";
            await storage.StoreRuleLinkAsync(item);
            storage.ForceBufferCallback();

            var items = (await storage.GetRuleLinksAsync(ruleId)).ToList();
            Assert.Single(items);
            Assert.Equal("b", items[0].Name);
        }

        private TKEpiserverBlobIPWhitelistLinkStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new TKEpiserverBlobIPWhitelistLinkStorage(factoryMock.Object, cache);
        }
    }
}
