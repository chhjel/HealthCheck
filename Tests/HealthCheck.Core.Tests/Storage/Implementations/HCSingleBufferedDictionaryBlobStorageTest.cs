using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class HCSingleBufferedDictionaryBlobStorageTest : HCSingleBufferedDictionaryBlobStorageBase<HCSingleBufferedDictionaryBlobStorageTest.TestData, TestItem, int>
	{
		public HCSingleBufferedDictionaryBlobStorageTest(IHCCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		public HCSingleBufferedDictionaryBlobStorageTest(IHCCache cache, ITestOutputHelper output) : base(cache)
		{
            Output = output;
			SupportsMaxItemAge = true;
        }

		protected override string CacheKey => "HCSingleBufferedDictionaryBlobStorageTest";

        public ITestOutputHelper Output { get; }

        public void Add(TestItem item) => InsertItemBuffered(item, item.Id);
		public TestData GetData() => GetBlobData();

        protected override TestData RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestData data) => Store(data);
		protected override DateTimeOffset GetItemTimestamp(TestItem item) => item.TimeStamp;

        public IEnumerable<TestItem> GetAllBufferedItems() => GetBufferedItems();

		public class TestData : IBufferedBlobDictionaryStorageData
        {
			public Dictionary<int, TestItem> Items { get; set; } = new Dictionary<int, TestItem>();
        }
	}
}
