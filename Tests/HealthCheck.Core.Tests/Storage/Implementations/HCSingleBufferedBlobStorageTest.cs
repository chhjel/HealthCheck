using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class HCSingleBufferedBlobStorageTest : HCSingleBufferedBlobStorageBase<HCSingleBufferedBlobStorageTest.TestData, TestItem>
	{
		public ITestOutputHelper Output { get; }
		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "HCSingleBufferedBlobStorageTest";

		public HCSingleBufferedBlobStorageTest(IHCCache cache, ITestOutputHelper output) : base(cache)
		{
			Output = output;
		}

		public void Add(TestItem item) => InsertItemBuffered(item, item.Id);
        public void Update(TestItem item) => UpdateItemBuffered(item.Id, x => x.Value = item.Value, () => GetData().Items.FirstOrDefault(x=>x.Id == item.Id));
		public TestData GetData() => GetBlobData();
		public IEnumerable<TestItem> GetAllBufferedItems() => GetBufferedItems();

		protected override TestData RetrieveBlobData() => Get();
        protected override void StoreBlobData(TestData data) => Store(data);
		protected override object GetItemId(TestItem item) => item.Id;

        protected override TestData UpdateDataFromBuffer(TestData data, Queue<BufferQueueItem> bufferedItems)
		{
			int updated = 0;
			int added = 0;
			foreach (var bufferItem in bufferedItems)
			{
				if (bufferItem.IsUpdate)
				{
					var existingIndex = data.Items.FindIndex(x => GetItemId(x).Equals(bufferItem.Id));
					if (existingIndex != -1)
					{
						data.Items.RemoveAt(existingIndex);
						data.Items.Insert(existingIndex, bufferItem.Item);
						updated++;
					}
				}
				else
				{
					data.Items.Add(bufferItem.Item);
					added++;
				}
			}

			Output.WriteLine($"UpdateDataFromBuffer - Buffer size: {bufferedItems.Count}, added: {added}, updated: {updated}");
			return data;
        }


		public class TestData
		{
			public List<TestItem> Items { get; set; } = new List<TestItem>();
		}
	}
}
