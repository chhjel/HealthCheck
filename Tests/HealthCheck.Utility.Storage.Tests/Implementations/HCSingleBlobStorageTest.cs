using HealthCheck.Utility.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace HealthCheck.Utility.Storage.Tests.Implementations
{
    public class HCSingleBlobStorageTest : HCSingleBlobStorageBase<TestItem>
	{
		public HCSingleBlobStorageTest(IMemoryCache cache) : base(cache) {}

		public Func<TestItem> Get { get; set; }
		public Action<TestItem> Store { get; set; }

		protected override string CacheKey => "HCSingleBlobStorageTest";
		protected override TestItem RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestItem data) => Store(data);
	}
}
