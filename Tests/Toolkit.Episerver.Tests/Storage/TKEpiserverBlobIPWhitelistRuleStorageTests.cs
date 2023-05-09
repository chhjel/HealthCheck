using QoDL.Toolkit.Episerver.IPWhitelist;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Storage
{
    public class TKEpiserverBlobIPWhitelistRuleStorageTests
    {
        [Fact]
        public async Task StoreRule_Twice_ShouldCreateTwoRules()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            TKIPWhitelistRule createRule(string name)
                => new TKIPWhitelistRule() { Name = name };

            await storage.StoreRuleAsync(createRule("a"));
            await storage.StoreRuleAsync(createRule("b"));
            storage.ForceBufferCallback();

            var items = (await storage.GetRulesAsync()).ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal("a", items[0].Name);
            Assert.Equal("b", items[1].Name);
        }

        [Fact]
        public async Task StoreRule_Update_ShouldUpdateExisting()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            TKIPWhitelistRule createRule(string name)
                => new TKIPWhitelistRule() { Name = name };

            var rule = await storage.StoreRuleAsync(createRule("a"));
            rule.Name = "b";
            await storage.StoreRuleAsync(rule);
            storage.ForceBufferCallback();

            var items = (await storage.GetRulesAsync()).ToList();
            Assert.Single(items);
            Assert.Equal("b", items[0].Name);
        }


        private TKEpiserverBlobIPWhitelistRuleStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new TKEpiserverBlobIPWhitelistRuleStorage(factoryMock.Object, cache);
        }
    }
}
