using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class HCSingleBufferedMultiListBlobStorageTest : HCSingleBufferedMultiListBlobStorageBase<HCSingleBufferedMultiListBlobStorageTest.TestData, TestItem, int>
	{
		public HCSingleBufferedMultiListBlobStorageTest(IHCCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "HCSingleBufferedMultiListBlobStorageTest";

		public void Add(TestItem item) => InsertItemBuffered(item);
		public TestData GetData() => GetBlobData();

		protected override TestData RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestData data) => Store(data);

		public class TestData : IBufferedBlobMultiListStorageData
		{
			public Dictionary<int, List<TestItem>> Lists { get; set; } = new Dictionary<int, List<TestItem>>();
        }
	}
}
