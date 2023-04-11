using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using System;

namespace QoDL.Toolkit.Core.Tests.Storage.Implementations
{
    public class TKSingleBlobStorageTest : TKSingleBlobStorageBase<TestItem>
	{
		public TKSingleBlobStorageTest(ITKCache cache) : base(cache) {}

		public Func<TestItem> Get { get; set; }
		public Action<TestItem> Store { get; set; }

		protected override string CacheKey => "TKSingleBlobStorageTest";
		protected override TestItem RetrieveBlobData() => Get();
		protected override void StoreBlobData(TestItem data) => Store(data);
	}
}
