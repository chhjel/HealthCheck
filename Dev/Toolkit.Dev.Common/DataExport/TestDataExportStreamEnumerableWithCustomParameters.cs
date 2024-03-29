using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.DataExport.TestDataExportStreamEnumerableWithCustomParameters;

namespace QoDL.Toolkit.Dev.Common.DataExport;

public class TestDataExportStreamEnumerableWithCustomParameters : TKDataExportStreamBase<TestExportItem, Parameters>
{
    public override string StreamDisplayName => "Enumerable stream with custom parameters";
    public override string StreamDescription => "A test for use during dev.";
    //public override string StreamGroupName => null;
    //public override object AllowedAccessRoles => null;
    public override List<string> Categories => new() { "Test category here" };
    public override int ExportBatchSize => 50000;

    protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(TKDataExportFilterDataTyped<TestExportItem, Parameters> filter)
    {
        var matches = Enumerable.Range(1, 1000000)
            .Select(x => new TestExportItem
            {
                Id = $"#{x}",
                Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                Value = x
            })
            .Where(x => string.IsNullOrWhiteSpace(filter.Parameters.StringFilter) || x.Name.Contains(filter.Parameters.StringFilter))
            .Where(x => filter.Parameters.MinValue == null || x.Value >= filter.Parameters.MinValue)
            .Where(x => filter.Parameters.MaxValue == null || x.Value <= filter.Parameters.MaxValue);

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

    public class Parameters
    {
        public DateTimeOffset DateTimeOffsetTest { get; set; }
        public DateTime DateTimeTest { get; set; }
        public DateTimeOffset DateTimeOffsetTestWDef { get; set; } = DateTimeOffset.Now.AddDays(-60);
        public DateTime DateTimeTestWDef { get; set; } = DateTime.Now.AddDays(-30);
        public DateTimeOffset DateTimeOffsetTestNow { get; set; } = DateTimeOffset.Now;
        public DateTime DateTimeTestMin { get; set; } = DateTime.MinValue;
        public string StringFilter { get; set; }
        public string StringFilterWDef { get; set; } = "Default stringPls";
        public int? MinValue { get; set; }
        [TKCustomProperty(NullName = "<9001>")]
        public int? MaxValue { get; set; } = 900001;
        public TestEnum SomeEnum { get; set; } = TestEnum.ValueA;
        [TKCustomProperty(NullName = "<Any>")]
        public TestEnum? SomeNullableEnum { get; set; } = TestEnum.ValueB;
        public List<TestEnum> SomeEnumList { get; set; }
        public float SomeFloat { get; set; } = 12.34f;
        public float? SomeNullableFloat { get; set; }
    }
    public enum TestEnum
    {
        ValueA, ValueB, ValueC
    }
}
