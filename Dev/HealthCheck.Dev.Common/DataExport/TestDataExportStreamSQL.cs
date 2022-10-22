using HealthCheck.Module.DataExport.Abstractions.Streams;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamSQL : SqlExportStreamBase
    {
        public override string StreamDisplayName => "SQL stream";
        public override string StreamDescription => "A test using SQL directly.";
        public override string StreamGroupName => null;
        public override object AllowedAccessRoles => RuntimeTestAccessRole.WebAdmins;
        public override List<string> Categories => null;
        public override int ExportBatchSize => 10000;

        public TestDataExportStreamSQL(ISqlExportStreamQueryExecutor queryExecutor) : base(queryExecutor)
        {
        }
    }

    public class TestSqlExportStreamQueryExecutor : ISqlExportStreamQueryExecutor
    {
        // TODO: ExecuteParameterizedQueryAsync(string query, int pageIndex, int pageSize, Dictionary<string, object> parameters)
        public async Task<SqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(SqlExportStreamQueryExecutorQueryModel model)
        {
            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=FeatureStudioDevNetFramework;Integrated Security=True;";
            var totalCount = ExecuteScalarInt(connectionString, $"SELECT COUNT(*)\n{model.QueryPredicate}");

            using var connection = new SqlConnection(connectionString);
            var query = $"{model.QuerySelect}\n{model.QueryPredicate}";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@pageIndex", model.PageIndex);
            command.Parameters.AddWithValue("@pageSize", model.PageSize);
            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            try
            {
                var table = new DataTable();
                table.Load(reader);

                // Read column data
                var columns = new List<SqlExportStreamQueryExecutorResultColumnModel>();
                foreach (var col in table.Columns.OfType<DataColumn>())
                {
                    columns.Add(new SqlExportStreamQueryExecutorResultColumnModel
                    {
                        Type = col.DataType,
                        Name = col.ColumnName
                    });
                }

                // Read rows data
                var rows = new List<List<object>>();
                foreach (var tableRow in table.Rows.Cast<DataRow>())
                {
                    rows.Add(tableRow.ItemArray.ToList());
                }

                return new SqlExportStreamQueryExecutorResultModel()
                {
                    TotalCount = totalCount,
                    RecordsAffected = reader.RecordsAffected,
                    Columns = columns,
                    Rows = rows
                };
            }
            finally
            {
                reader.Close();
            }
        }

        private int ExecuteScalarInt(string connectionString, string query)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
