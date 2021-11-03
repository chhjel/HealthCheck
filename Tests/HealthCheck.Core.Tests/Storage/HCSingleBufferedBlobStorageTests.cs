using HealthCheck.Core.Tests.Storage.Implementations;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.Core.Tests.Storage
{
    public class HCSingleBufferedBlobStorageTests
    {
        public ITestOutputHelper Output { get; }

        public HCSingleBufferedBlobStorageTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void InsertBuffered_StoresWhenItShould()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromMilliseconds(150),
                MaxBufferSize = 30000,
                Get = () => { getCounter++; return new HCSingleBufferedBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            for (int i = 0; i < 50000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}" });
            }

            var data = storage.GetData();
            Assert.NotNull(data);
            Assert.Equal(1, getCounter);
            Assert.Equal(1, setCounter);
            Assert.Equal(30000, data.Items.Count);

            storage.ForceBufferCallback();
            Assert.Equal(2, setCounter);
            Assert.Equal(50000, data.Items.Count);
            Assert.Empty(storage.GetAllBufferedItems());
        }

        [Fact]
        public void UpdateBuffered_StoresWhenItShould()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromSeconds(5),
                MaxBufferSize = 1000,
                Get = () => { getCounter++; return new HCSingleBufferedBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            Output.WriteLine($"Add {1100}");
            for (int i = 0; i < 1100; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}" });
            }

            var data = storage.GetData();
            Assert.NotNull(data);
            Assert.Equal(1, getCounter); // Initial get
            Assert.Equal(1, setCounter); // 1 set to store buffer overflow
            Assert.Equal(1000, data.Items.Count);
            Assert.Equal(100, storage.GetAllBufferedItems().Count());

            Output.WriteLine($"ForceBufferCallback()");
            storage.ForceBufferCallback();
            Assert.Empty(storage.GetAllBufferedItems());
            Assert.Equal(2, setCounter);
            Assert.Equal(1100, data.Items.Count);

            Output.WriteLine($"Update {1000}");
            for (int i = 0; i < 1100; i++)
            {
                storage.Update(new TestItem { Id = i, Value = $"Item #{i} - Updated!" });
            }

            Output.WriteLine($"ForceBufferCallback()");
            storage.ForceBufferCallback();
            var updatedCount = data.Items.Count(x => x.Value.EndsWith(" - Updated!"));
            var notUpdatedCount = data.Items.Count(x => !x.Value.EndsWith(" - Updated!"));
            Output.WriteLine($"updatedCount: {updatedCount}");
            Output.WriteLine($"notUpdatedCount: {notUpdatedCount}");

            Assert.Equal(1, getCounter);
            Assert.Equal(4, setCounter); // inserts (1000+100) + update (1000+100)
            Assert.Equal(1100, data.Items.Count);
            Assert.Empty(storage.GetAllBufferedItems());
            Assert.True(data.Items.TrueForAll(x => x.Value.EndsWith(" - Updated!")));
            Assert.True(!data.Items.GroupBy(x => x.Value).Any(x => x.Count() > 1));
        }
    }
}
