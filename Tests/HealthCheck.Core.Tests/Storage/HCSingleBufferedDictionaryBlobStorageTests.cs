using HealthCheck.Core.Tests.Storage.Implementations;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.Core.Tests.Storage
{
    public class HCSingleBufferedDictionaryBlobStorageTests
    {
        public ITestOutputHelper Output { get; }

        public HCSingleBufferedDictionaryBlobStorageTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Add_StoresValues()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedDictionaryBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromDays(1),
                MaxBufferSize = 5,
                Get = () => { getCounter++; return new HCSingleBufferedDictionaryBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            for (int i = 0; i < 8; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now });
            }

            var data = storage.GetData();
            Assert.NotNull(data);
            Assert.Equal(1, getCounter);
            Assert.Equal(1, setCounter);
            Assert.Equal(5, data.Items.Count);
            Assert.Equal(3, storage.GetAllBufferedItems().Count());

            storage.ForceBufferCallback();
            Assert.Equal(2, setCounter);
            Assert.Equal(8, data.Items.Count);
            Assert.Empty(storage.GetAllBufferedItems());
        }

        [Fact]
        public void Add_WithUpdates_PerformsUpdate()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedDictionaryBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromDays(1),
                MaxBufferSize = 10000,
                Get = () => { getCounter++; return new HCSingleBufferedDictionaryBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            Output.WriteLine($"Add {11000}");
            for (int i = 0; i < 11000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now });
            }

            var data = storage.GetData();
            Assert.NotNull(data);
            Assert.Equal(1, getCounter); // Initial get
            Assert.Equal(1, setCounter); // 1 set to store buffer overflow
            Assert.Equal(10000, data.Items.Count);
            Assert.Equal(1000, storage.GetAllBufferedItems().Count());

            Output.WriteLine($"ForceBufferCallback()");
            storage.ForceBufferCallback();
            Assert.Empty(storage.GetAllBufferedItems());
            Assert.Equal(2, setCounter);
            Assert.Equal(11000, data.Items.Count);

            Output.WriteLine($"Update {11000}");
            for (int i = 0; i < 11000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i} - Updated!", TimeStamp = DateTimeOffset.Now });
            }

            Output.WriteLine($"ForceBufferCallback()");
            storage.ForceBufferCallback();
            var updatedCount = data.Items.Count(x => x.Value.Value.EndsWith(" - Updated!"));
            var notUpdatedCount = data.Items.Count(x => !x.Value.Value.EndsWith(" - Updated!"));
            Output.WriteLine($"updatedCount: {updatedCount}");
            Output.WriteLine($"notUpdatedCount: {notUpdatedCount}");

            Assert.Equal(1, getCounter);
            Assert.Equal(4, setCounter); // inserts (10000+1000) + update (10000+1000)
            Assert.Equal(11000, data.Items.Count);
            Assert.Empty(storage.GetAllBufferedItems());
            Assert.True(data.Items.Values.ToList().TrueForAll(x => x.Value.EndsWith(" - Updated!")));
            Assert.True(!data.Items.GroupBy(x => x.Value).Any(x => x.Count() > 1));
        }

        [Fact]
        public void Add_WithMaxSize_RespectsMaxSize()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedDictionaryBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromDays(1),
                MaxBufferSize = 30000,
                MaxItemCount = 20000,
                Get = () => { getCounter++; return new HCSingleBufferedDictionaryBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            for (int i = 0; i < 50000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now });
            }

            storage.ForceBufferCallback();
            var data = storage.GetData();
            Assert.Equal(20000, data.Items.Count);
            Assert.Empty(storage.GetAllBufferedItems());
        }

        [Fact]
        public void Add_WithMaxAge_RespectsMaxAge()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedDictionaryBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromDays(1),
                MaxBufferSize = 1000,
                MaxItemAge = TimeSpan.FromDays(30),
                Get = () => { getCounter++; return new HCSingleBufferedDictionaryBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            for (int i = 0; i < 5000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now });
            }

            for (int i = 0; i < 2000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now.AddDays(-60) });
            }

            storage.ForceBufferCallback();
            var data = storage.GetData();
            Assert.Equal(3000, data.Items.Count); // 5000 - 2000 expired
            Assert.Empty(storage.GetAllBufferedItems());
        }

        [Fact]
        public void Add_WithMaxAgeBuffered_RespectsMaxAge()
        {
            var cache = new TestCache();
            var getCounter = 0;
            var setCounter = 0;
            var storage = new HCSingleBufferedDictionaryBlobStorageTest(cache, Output)
            {
                BlobUpdateBufferDuration = TimeSpan.FromDays(1),
                MaxBufferSize = 30000,
                MaxItemAge = TimeSpan.FromDays(30),
                Get = () => { getCounter++; return new HCSingleBufferedDictionaryBlobStorageTest.TestData(); },
                Store = (d) => { setCounter++; }
            };

            for (int i = 0; i < 5000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now });
            }

            for (int i = 0; i < 2000; i++)
            {
                storage.Add(new TestItem { Id = i, Value = $"Item #{i}", TimeStamp = DateTimeOffset.Now.AddDays(-60) });
            }

            storage.ForceBufferCallback();
            var data = storage.GetData();
            Assert.Equal(3000, data.Items.Count); // 5000 - 2000 expired
            Assert.Empty(storage.GetAllBufferedItems());
        }
    }
}
