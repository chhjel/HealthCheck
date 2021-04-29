using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using System;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class HCSingleBlobStorageTest : HCSingleBlobStorageBase<TestItem>
	{
		public HCSingleBlobStorageTest(IHCCache cache) : base(cache) {}

		public Func<TestItem> Get { get; set; }
		public Action<TestItem> Store { get; set; }

		protected override string CacheKey => "HCSingleBlobStorageTest";
		protected override TestItem RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestItem data) => Store(data);
	}
}
