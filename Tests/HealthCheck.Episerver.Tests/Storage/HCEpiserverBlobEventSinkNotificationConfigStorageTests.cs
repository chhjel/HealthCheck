using HealthCheck.Core.Modules.EventNotifications.Models;
using HealthCheck.Episerver.Storage;
using HealthCheck.Episerver.Tests.Helpers;
using System;
using System.Linq;
using Xunit;

namespace HealthCheck.Episerver.Tests.Storage
{
    public class HCEpiserverBlobEventSinkNotificationConfigStorageTests
    {
        [Fact]
        public void SaveConfig_WhenBuffered_ItemsShouldHaveUniqueIds()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);

            for (int i = 0; i < 100; i++)
            {
                var config = new EventSinkNotificationConfig
                {
                    Enabled = true
                };
                storage.SaveConfig(config);
            }

            var items = storage.GetConfigs().ToList();
            Assert.True(items.TrueForAll(x => x.Id != Guid.Empty), "All items should have generated ids");
            Assert.True(items.GroupBy(x => x.Id).All(x => x.Count() == 1), "All items should have unique ids");
        }

        [Fact]
        public void SaveConfig_WhenBuffered_ShouldBeAvailableAtOnce()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            for (int i = 0; i < 100; i++)
            {
                var config = new EventSinkNotificationConfig
                {
                    Enabled = true
                };
                storage.SaveConfig(config);
            }

            var items = storage.GetConfigs().ToList();
            Assert.Equal(100, items.Count); // Should be 100 items stored
        }

        [Fact]
        public void SaveConfig_UpdatingBuffered_ShouldBeUpdated()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            for (int i = 0; i < 10; i++) storage.SaveConfig(new EventSinkNotificationConfig());
            var insertedConfig = storage.SaveConfig(new EventSinkNotificationConfig { Enabled = true, LastChangedBy = "before" });
            for (int i = 0; i < 10; i++) storage.SaveConfig(new EventSinkNotificationConfig());

            Assert.Equal("before", insertedConfig.LastChangedBy); // Should return inserted value
            var matches = storage.GetConfigs().Where(x => x.Id == insertedConfig.Id);
            Assert.Equal("before", matches.Single().LastChangedBy); // Should have buffered inserted value

            insertedConfig.LastChangedBy = "after";
            storage.SaveConfig(insertedConfig);
            matches = storage.GetConfigs().Where(x => x.Id == insertedConfig.Id);
            Assert.Equal("after", matches.Single().LastChangedBy); // Should have buffered updated value

            // Simulate buffer duration elapsed
            storage.ForceBufferCallback();
            Assert.Equal("after", storage.GetConfigs().Single(x => x.Id == insertedConfig.Id).LastChangedBy); // Should have stored updated value
        }

        [Fact]
        public void SaveConfig_UpdatingStored1_ShouldBeUpdated()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            for (int i = 0; i < 10; i++) storage.SaveConfig(new EventSinkNotificationConfig());
            var insertedConfig = storage.SaveConfig(new EventSinkNotificationConfig { Enabled = true, LastChangedBy = "before" });
            for (int i = 0; i < 10; i++) storage.SaveConfig(new EventSinkNotificationConfig());

            Assert.Equal("before", insertedConfig.LastChangedBy); // Should return inserted value
            Assert.Equal("before", storage.GetConfigs().Single(x => x.Id == insertedConfig.Id).LastChangedBy); // Should have buffered inserted value
            storage.ForceBufferCallback();

            insertedConfig.LastChangedBy = "after";
            storage.ForceBufferCallback();
            storage.SaveConfig(insertedConfig);
            Assert.Equal("after", storage.GetConfigs().Single(x => x.Id == insertedConfig.Id).LastChangedBy); // Should have stored updated value
        }

        [Fact]
        public void SaveConfig_UpdatingStored2_ShouldBeUpdated()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            for (int i = 0; i < 10; i++) storage.SaveConfig(new EventSinkNotificationConfig());
            var insertedConfig = storage.SaveConfig(new EventSinkNotificationConfig { Enabled = true, LastChangedBy = "before" });
            for (int i = 0; i < 10; i++) storage.SaveConfig(new EventSinkNotificationConfig());

            Assert.Equal("before", insertedConfig.LastChangedBy); // Should return inserted value
            Assert.Equal("before", storage.GetConfigs().Single(x => x.Id == insertedConfig.Id).LastChangedBy); // Should have buffered inserted value
            storage.ForceBufferCallback();

            insertedConfig.LastChangedBy = "after";
            storage.ForceBufferCallback();
            storage.SaveConfig(insertedConfig);
            storage.ForceBufferCallback();
            Assert.Equal("after", storage.GetConfigs().Single(x => x.Id == insertedConfig.Id).LastChangedBy); // Should have stored updated value
        }

        private HCEpiserverBlobEventSinkNotificationConfigStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new HCEpiserverBlobEventSinkNotificationConfigStorage(factoryMock.Object, cache);
        }
    }
}
