using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class HCSingleBufferedDictionaryBlobStorageTest : HCSingleBufferedDictionaryBlobStorageBase<HCSingleBufferedDictionaryBlobStorageTest.TestData, TestItem, int>
	{
		public HCSingleBufferedDictionaryBlobStorageTest(IHCCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "HCSingleBufferedDictionaryBlobStorageTest";

		public void Add(TestItem item) => InsertItemBuffered(item);
		public TestData GetData() => GetBlobData();

		protected override int GetItemId(TestItem item) => item.Id;
        protected override TestData RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestData data) => Store(data);

        public class TestData : IBufferedBlobDictionaryStorageData
        {
			public Dictionary<int, TestItem> Items { get; set; } = new Dictionary<int, TestItem>();
        }
	}
}
