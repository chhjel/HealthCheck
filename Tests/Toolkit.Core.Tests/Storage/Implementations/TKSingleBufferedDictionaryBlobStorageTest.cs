using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Storage.Implementations
{
    public class TKSingleBufferedDictionaryBlobStorageTest : TKSingleBufferedDictionaryBlobStorageBase<TKSingleBufferedDictionaryBlobStorageTest.TestData, TestItem, int>
	{
		public TKSingleBufferedDictionaryBlobStorageTest(ITKCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		public TKSingleBufferedDictionaryBlobStorageTest(ITKCache cache, ITestOutputHelper output) : base(cache)
		{
            Output = output;
			SupportsMaxItemAge = true;
        }

		protected override string CacheKey => "TKSingleBufferedDictionaryBlobStorageTest";

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
