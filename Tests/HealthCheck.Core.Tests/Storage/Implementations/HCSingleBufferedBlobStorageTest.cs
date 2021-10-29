﻿using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class HCSingleBufferedBlobStorageTest : HCSingleBufferedBlobStorageBase<HCSingleBufferedBlobStorageTest.TestData, TestItem>
	{
		public HCSingleBufferedBlobStorageTest(IHCCache cache) : base(cache) { }

		public Func<TestData> Get { get; set; }
		public Action<TestData> Store { get; set; }

		protected override string CacheKey => "HCSingleBufferedBlobStorageTest";

		public void Add(TestItem item) => InsertItemBuffered(item);
        public void Update(TestItem item) => UpdateItemBuffered(item.Id, x => x.Value = item.Value);
		public TestData GetData() => GetBlobData();

		protected override TestData RetrieveBlobData() => Get();
        protected override void StoreBlobData(TestData data) => Store(data);

		protected override TestData UpdateDataFromBuffer(TestData data, Queue<BufferQueueItem> bufferedItems)
        {
            var toInsert = bufferedItems.Where(x => x.IsInsert).Select(x => x.ItemToInsert);
            data.Items.AddRange(toInsert.Select(x => x));

            var toUpdate = bufferedItems.Where(x => x.IsUpdate && x.Id is int);
            foreach (var action in toUpdate)
            {
                var existing = data.Items.FirstOrDefault(x => x.Id == (int)action.Id);
                if (existing != null)
                {
                    action.UpdateAction(existing);
                }
            }

			return data;
        }

        public class TestData
		{
			public List<TestItem> Items { get; set; } = new List<TestItem>();
		}
	}
}
