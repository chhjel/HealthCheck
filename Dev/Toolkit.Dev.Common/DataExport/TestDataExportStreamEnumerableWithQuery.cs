using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.DataExport.TestDataExportStreamEnumerableWithQuery;

namespace QoDL.Toolkit.Dev.Common.DataExport;

public class TestDataExportStreamEnumerableWithQuery : TKDataExportStreamBase<TestExportItem>
{
    public override string StreamDisplayName => "Enumerable stream with query";
    public override string StreamDescription => "A test for use during dev.";
    //public override string StreamGroupName => null;
    //public override object AllowedAccessRoles => null;
    public override List<string> Categories => new() { "Test category here" };
    public override int ExportBatchSize => 50000;
    public override ITKDataExportStream.QueryMethod Method => ITKDataExportStream.QueryMethod.Enumerable;
    public override bool SupportsQuery() => true;

    protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(TKDataExportFilterDataTyped<TestExportItem> filter)
    {
        var matches = Enumerable.Range(1, 1000000)
            .Select(x => new TestExportItem
            {
                Id = $"#{x}",
                Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                Value = x
            })
            .Where(filter.QueryPredicate);

        var pageItems = matches
            .Skip(filter.PageIndex * filter.PageSize)
            .Take(filter.PageSize);

        return Task.FromResult(new TypedEnumerableResult
        {
            PageItems = pageItems,
            TotalCount = matches.Count()
        });
    }

    public class TestExportItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
