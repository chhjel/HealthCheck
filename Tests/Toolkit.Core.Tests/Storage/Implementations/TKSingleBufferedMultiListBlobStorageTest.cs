using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Tests.Storage.Implementations;

public class TKSingleBufferedMultiListBlobStorageTest : TKSingleBufferedMultiListBlobStorageBase<TKSingleBufferedMultiListBlobStorageTest.TestData, TestItem, int>
{
    public TKSingleBufferedMultiListBlobStorageTest(ITKCache cache) : base(cache) { }

    public Func<TestData> Get { get; set; }
    public Action<TestData> Store { get; set; }

    protected override string CacheKey => "TKSingleBufferedMultiListBlobStorageTest";

    public void Add(TestItem item) => InsertItemBuffered(item, item.Id);
    public TestData GetData() => GetBlobData();

    protected override TestData RetrieveBlobData() => Get();
    protected override void StoreBlobData(TestData data) => Store(data);

    public class TestData : IBufferedBlobMultiListStorageData
    {
        public Dictionary<int, List<TestItem>> Lists { get; set; } = new Dictionary<int, List<TestItem>>();
    }
}
