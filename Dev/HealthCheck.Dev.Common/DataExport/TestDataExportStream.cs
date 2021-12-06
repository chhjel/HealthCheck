using HealthCheck.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStream : IHCDataExportStream
    {
        public string StreamDisplayName => "Test stream";
        public string StreamDescription => "A test for use during dev.";
        public string StreamGroupName => "Dev streams";
        public object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
        public List<string> Categories => null;

        public Task<IQueryable<object>> GetQueryableAsync()
        {
            var items = Enumerable.Range(0, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x*100}",
                    Value = x
                })
                .Cast<object>();
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
