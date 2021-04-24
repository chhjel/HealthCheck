using HealthCheck.Utility.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Utility.Storage.Tests.Implementations
{
    public class HCSingleBufferedBlobStorageTest : HCSingleBufferedBlobStorageBase<HCSingleBufferedBlobStorageTest.TestData, TestItem>
	{
		public HCSingleBufferedBlobStorageTest(IMemoryCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "HCSingleBufferedBlobStorageTest";

		public void Add(TestItem item) => InsertItemBuffered(item);
		public TestData GetData() => GetBlobData();

		protected override TestData RetrieveBlobData() => Get();
        protected override void StoreBlobData(TestData data) => Store(data);
		protected override TestData UpdateDataFromBuffer(TestData data, Queue<BufferQueueItem> bufferedItems)
        {
			data.Items.AddRange(bufferedItems.Select(x => x.Item));
			return data;
        }

        public class TestData
		{
			public List<TestItem> Items { get; set; } = new List<TestItem>();
		}
	}
}
