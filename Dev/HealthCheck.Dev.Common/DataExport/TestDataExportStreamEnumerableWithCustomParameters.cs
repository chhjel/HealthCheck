using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Formatters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Dev.Common.DataExport.TestDataExportStreamEnumerableWithCustomParameters;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamEnumerableWithCustomParameters : HCDataExportStreamBase<TestExportItem, TestDataExportStreamEnumerableWithCustomParameters.Parameters>
    {
        public override string StreamDisplayName => "Enumerable stream with parameters";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        public override List<string> Categories => new List<string> { "Test category here" };
        public override int ExportBatchSize => 50000;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsWithCustomFilterAsync(int pageIndex, int pageSize, Parameters parameters)
        {
            var matches = Enumerable.Range(1, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x
                })
                .Where(x => string.IsNullOrWhiteSpace(parameters.StringFilter) || x.Name.Contains(parameters.StringFilter))
                .Where(x => parameters.MinValue == null || x.Value >= parameters.MinValue)
                .Where(x => parameters.MaxValue == null || x.Value <= parameters.MaxValue);

            var pageItems = matches
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

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
            public string StringFilter { get; set; }
            public int? MinValue { get; set; }
            public int? MaxValue { get; set; }
        }
    }
}
