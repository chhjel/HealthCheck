using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Storage.Implementations
{
    public class TKSingleBufferedListBlobStorageTest : TKSingleBufferedListBlobStorageBase<TKSingleBufferedListBlobStorageTest.TestData, TestItem>
	{
		public TKSingleBufferedListBlobStorageTest(ITKCache cache, ITestOutputHelper output) : base(cache)
		{
            Output = output;
        }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "TKSingleBufferedListBlobStorageTest";

        public ITestOutputHelper Output { get; }

        public void Add(TestItem item) => InsertItemBuffered(item, item.Id);
		public TestData GetData() => GetBlobData();

		protected override TestData RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestData data) => Store(data);

		public class TestData : IBufferedBlobListStorageData
		{
			public List<TestItem> Items { get; set; } = new List<TestItem>();
		}
	}
}
