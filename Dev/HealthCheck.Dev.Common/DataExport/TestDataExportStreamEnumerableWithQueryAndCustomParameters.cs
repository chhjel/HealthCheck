using HealthCheck.Core.Attributes;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Dev.Common.DataExport.TestDataExportStreamEnumerableWithQueryAndCustomParameters;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamEnumerableWithQueryAndCustomParameters : HCDataExportStreamBase<TestExportItem, Parameters>
    {
        public override string StreamDisplayName => "Enumerable stream with query and custom parameters";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        public override List<string> Categories => new List<string> { "Test category here" };
        public override int ExportBatchSize => 50000;
        public override bool SupportsQuery() => true;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<TestExportItem, Parameters> filter)
        {
            var matches = Enumerable.Range(1, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x
                })
                .Where(filter.QueryPredicate)
                .Take(filter.Parameters.MaxResults);

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
            [HCCustomProperty(NullName = "Oldest")]
            public DateTime? From { get; set; } = DateTime.Now.AddDays(-30);

            [HCCustomProperty(NullName = "Newest")]
            public DateTime? To { get; set; }

            [HCCustomProperty(Description = "Limit max results in order to gain some performance.")]
            public int MaxResults { get; set; } = 1000;
        }
    }
}
