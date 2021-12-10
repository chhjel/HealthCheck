using HealthCheck.Module.DataExport.Abstractions;
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
        public override string StreamGroupName => null;
        public override object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
        public override List<string> Categories => null;
        public override int ExportBatchSize => 500;

        protected override Task<IQueryable<TestExportItem>> GetQueryableItemsAsync()
        {
            var items = Enumerable.Range(0, 1000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}",
                    Value = x
                });
            return Task.FromResult(items.AsQueryable());
        }

        public class TestExportItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}
