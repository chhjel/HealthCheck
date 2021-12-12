using HealthCheck.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Dev.Common.DataExport.TestDataExportStream2;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStream2 : HCDataExportStreamBase<TestExportItem>
    {
        public override string StreamDisplayName => "Test stream 2";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        //public override List<string> Categories => null;
        public override int ExportBatchSize => 500;
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Enumerable;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(int pageIndex, int pageSize, Func<TestExportItem, bool> predicate)
        {
            var matches = Enumerable.Range(1, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x
                })
                .Where(predicate);

            var pageItems = matches
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

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
        }
    }
}
