using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Abstractions.Streams;
using HealthCheck.Module.DataExport.Models;
using System.Collections.Generic;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamSQL : HCSqlExportStreamBase<HCSqlExportStreamParameters>
    {
        public override string StreamDisplayName => "SQL stream";
        public override string StreamDescription => "A test using SQL directly.";
        public override string StreamGroupName => null;
        public override object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
        public override List<string> Categories => null;
        public override int ExportBatchSize => 10000;
        protected override List<ConnectionStringData> ConnectionStrings { get; } = new List<ConnectionStringData>
        {
            new ConnectionStringData
            {
                Label = "Testing",
                ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=FeatureStudioDevNetFramework;Integrated Security=True;"
            }
        };

        public TestDataExportStreamSQL(IHCSqlExportStreamQueryExecutor queryExecutor) : base(queryExecutor)
        {
        }
    }
}
