using HealthCheck.Core.Tests.Storage.Implementations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCheck.Core.Tests.Storage
{
    public class HCSingleBufferedBlobStorageTests
    {
        [Fact]
        public async Task InsertBuffered_StoresWhenItShould()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedBlobStorageTest(cache)
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

            await Task.Delay(TimeSpan.FromMilliseconds(300));
            Assert.Equal(2, setCounter);
            Assert.Equal(50000, data.Items.Count);
        }

        [Fact]
        public async Task UpdateBuffered_StoresWhenItShould()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedBlobStorageTest(cache)
            {
                BlobUpdateBufferDuration = TimeSpan.FromMilliseconds(150),
                MaxBufferSize = 10000,
                Get = () => { getCounter++; return new HCSingleBufferedBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            for (int i = 0; i < 11000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}" });
            }

            var data = storage.GetData();
            Assert.NotNull(data);
            Assert.Equal(1, getCounter);
            Assert.Equal(1, setCounter);
            Assert.Equal(10000, data.Items.Count);

            await Task.Delay(TimeSpan.FromMilliseconds(300));
            Assert.Equal(2, setCounter);
            Assert.Equal(11000, data.Items.Count);

            for (int i = 0; i < 11000; i++)
            {
                storage.Update(new TestItem { Id = i, Value = $"Item #{i} - Updated!" });
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            Assert.Equal(4, setCounter);
            Assert.True(data.Items.TrueForAll(x => x.Value.EndsWith(" - Updated!")));
            Assert.True(!data.Items.GroupBy(x => x.Value).Any(x => x.Count() > 1));
            Assert.Equal(11000, data.Items.Count);
        }
    }
}
