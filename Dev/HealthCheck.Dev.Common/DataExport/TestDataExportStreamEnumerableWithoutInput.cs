using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Dev.Common.DataExport.TestDataExportStreamEnumerableWithoutInput;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamEnumerableWithoutInput : HCDataExportStreamBase<TestExportItem>
    {
        public override string StreamDisplayName => "Enumerable stream without input";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        public override List<string> Categories => new List<string> { "Test category here" };
        public override int ExportBatchSize => 50000;
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Enumerable;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<TestExportItem> filter)
        {
            var matches = Enumerable.Range(1, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x,
                    ListOfStrings = Enumerable.Range(1, 5).Select(s => $"SubItem{s}").ToList(),
                    ArrayOfInt = Enumerable.Range(55, 3).ToArray()
                })
                .Where(filter.QueryPredicate);

            var pageItems = matches
                .Skip(filter.PageIndex * filter.PageSize)
                .Take(filter.PageSize);

            return Task.FromResult(new HCDataExportStreamBase<TestExportItem>.TypedEnumerableResult
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
            public List<string> ListOfStrings { get; set; }
            public int[] ArrayOfInt { get; set; }
        }
    }
}
