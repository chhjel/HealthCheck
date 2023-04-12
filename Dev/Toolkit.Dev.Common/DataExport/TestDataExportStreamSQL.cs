using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Abstractions.Streams;
using QoDL.Toolkit.Module.DataExport.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Dev.Common.DataExport;

public class TestDataExportStreamSQL : TKSqlExportStreamBase<TKSqlExportStreamParameters>
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
        },
        new ConnectionStringData
        {
            Label = "Broken",
            ConnectionString = @"not_working"
        }
    };

    public TestDataExportStreamSQL(ITKSqlExportStreamQueryExecutor queryExecutor) : base(queryExecutor)
    {
    }
}
