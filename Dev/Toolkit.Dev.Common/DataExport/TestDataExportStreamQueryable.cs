using QoDL.Toolkit.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.DataExport.TestDataExportStreamQueryable;

namespace QoDL.Toolkit.Dev.Common.DataExport;

public class TestDataExportStreamQueryable : TKDataExportStreamBase<TestExportItem>
{
    public override string StreamDisplayName => "Queryable stream";
    public override string StreamDescription => "A test for use during dev.";
    public override string StreamGroupName => null;
    public override object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
    public override List<string> Categories => null;
    public override int ExportBatchSize => 500;
    public override ITKDataExportStream.QueryMethod Method => ITKDataExportStream.QueryMethod.Queryable;

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

    protected override object DefaultFormatValue<T>(string propertyName, T value)
    {
        if (value is DateTimeOffset date)
        {
            return date.ToString("dd/MM/yyyy");
        }
        else if (value is DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }
        return value;
    }

    public class TestExportItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public DateTimeOffset? NullableDateTimeOffset { get; set; } = DateTimeOffset.Now;
        public DateTime? NullableTimestamp { get; set; } = DateTime.Now;
        public TestExportItemAddress BillingAddress { get; set; } = new TestExportItemAddress();
        public TestExportItemAddress ShippingAddress { get; set; } = new TestExportItemAddress();
        public TestExportItemContact Contact { get; set; } = new TestExportItemContact();
        public List<TestExportItemContact> ComplexList { get; set; } = new List<TestExportItemContact>()
        {
            new TestExportItemContact(),
            new TestExportItemContact(),
            new TestExportItemContact()
        };
        public string[] SimpleArray { get; set; } = new[] { "A", "B", "C" };

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
