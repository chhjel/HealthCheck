using HealthCheck.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Dev.Common.DataExport.TestDataExportStream1;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStream1 : HCDataExportStreamBase<TestExportItem>
    {
        public override string StreamDisplayName => "Test stream 1";
        public override string StreamDescription => "A test for use during dev.";
        public override string StreamGroupName => null;
        public override object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
        public override List<string> Categories => null;

        protected override Task<IQueryable<TestExportItem>> GetQueryableItemsAsync()
        {
            var items = Enumerable.Range(0, 10000)
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
            public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public TestExportItemAddress BillingAddress { get; set; } = new TestExportItemAddress();
            public TestExportItemAddress ShippingAddress { get; set; } = new TestExportItemAddress();
            public TestExportItemContact Contact { get; set; } = new TestExportItemContact();
        }

        public class TestExportItemAddress
        {
            public string City { get; set; } = "Devcity";
            public string Street { get; set; } = "Devstreet";
            public int Number { get; set; } = 88;
        }

        public class TestExportItemContact
        {
            public string FirstName { get; set; } = "Jimmy";
            public string LastName { get; set; } = "Smithy";
            public string FullName => $"{FirstName} {LastName}".Trim();
            public TestExportItemAddress HomeAddress { get; set; } = new TestExportItemAddress();
        }
    }
}
