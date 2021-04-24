using HealthCheck.Utility.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace HealthCheck.Utility.Storage.Tests.Implementations
{
    public class HCSingleBufferedListBlobStorageTest : HCSingleBufferedListBlobStorageBase<HCSingleBufferedListBlobStorageTest.TestData, TestItem>
	{
		public HCSingleBufferedListBlobStorageTest(IMemoryCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "HCSingleBufferedListBlobStorageTest";

		public void Add(TestItem item) => InsertItemBuffered(item);
		public TestData GetData() => GetBlobData();

		protected override TestData RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestData data) => Store(data);

		public class TestData : IBufferedBlobListStorageData
		{
			public List<TestItem> Items { get; set; } = new List<TestItem>();
		}
	}
}
