using HealthCheck.Module.DataExport.Abstractions.Streams;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamSQL : SqlExportStreamBase
    {
        public override string StreamDisplayName => "SQL stream";
        public override string StreamDescription => "A test using SQL directly.";
        public override string StreamGroupName => null;
        public override object AllowedAccessRoles => RuntimeTestAccessRole.QuerystringTest;
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
            
            var totalCountQuery = model.QuerySelectTotalCount.Replace("[PREDICATE]", model.QueryPredicate);
            var totalCount = ExecuteScalarInt(connectionString, totalCountQuery, model, setCommandParameters);

            var dataQuery = model.QuerySelectData.Replace("[PREDICATE]", model.QueryPredicate);
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(dataQuery, connection);
            setCommandParameters(model, command);
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

            static void setCommandParameters(SqlExportStreamQueryExecutorQueryModel model, SqlCommand command)
            {
                command.Parameters.AddWithValue("@pageIndex", model.PageIndex);
                command.Parameters.AddWithValue("@pageSize", model.PageSize);
                command.Parameters.AddWithValue("@skipCount", model.PageIndex * model.PageSize);
                command.Parameters.AddWithValue("@takeCount", model.PageSize);
            }
        }

        private int ExecuteScalarInt(string connectionString, string query, SqlExportStreamQueryExecutorQueryModel model, Action<SqlExportStreamQueryExecutorQueryModel, SqlCommand> parameterSetter)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection);
            parameterSetter(model, command);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
