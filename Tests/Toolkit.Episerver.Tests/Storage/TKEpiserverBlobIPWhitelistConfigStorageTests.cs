using QoDL.Toolkit.Episerver.IPWhitelist;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Storage
{
    public class TKEpiserverBlobIPWhitelistConfigStorageTests
    {
        [Fact]
        public async Task StoreConfig_Twice_ShouldResultInSingleConfig()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            TKIPWhitelistConfig createConfig(string value)
                => new TKIPWhitelistConfig() { DefaultResponse = value };

            await storage.SaveConfigAsync(createConfig("a"));
            await storage.SaveConfigAsync(createConfig("b"));
            storage.ForceBufferCallback();

            var config = await storage.GetConfigAsync();
            Assert.NotNull(config);
            Assert.Equal("b", config.DefaultResponse);
        }

        [Fact]
        public async Task StoreConfig_WithBuffer_ShouldResultInSingleConfig()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            TKIPWhitelistConfig createConfig(string value)
                => new TKIPWhitelistConfig() { DefaultResponse = value };

            await storage.SaveConfigAsync(createConfig("a"));
            await storage.SaveConfigAsync(createConfig("b"));

            var config = await storage.GetConfigAsync();
            Assert.NotNull(config);
            Assert.Equal("b", config.DefaultResponse);

            storage.ForceBufferCallback();

            config = await storage.GetConfigAsync();
            Assert.NotNull(config);
            Assert.Equal("b", config.DefaultResponse);

            config.DefaultResponse = "c";
            await storage.SaveConfigAsync(config);

            config = await storage.GetConfigAsync();
            Assert.NotNull(config);
            Assert.Equal("c", config.DefaultResponse);
        }

        private TKEpiserverBlobIPWhitelistConfigStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new TKEpiserverBlobIPWhitelistConfigStorage(factoryMock.Object, cache);
        }
    }
}
