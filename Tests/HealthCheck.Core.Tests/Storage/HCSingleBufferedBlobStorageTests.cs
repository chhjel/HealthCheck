using HealthCheck.Core.Tests.Storage.Implementations;
using System;
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
    }
}