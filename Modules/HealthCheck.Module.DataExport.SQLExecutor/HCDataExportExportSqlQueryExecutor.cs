using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.SQLExecutor
{
    /// <summary></summary>
    public class HCDataExportExportSqlQueryExecutor : IHCSqlExportStreamQueryExecutor
    {
        /// <summary></summary>
        public async Task<HCSqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(HCSqlExportStreamQueryExecutorQueryModel model)
        {
            string connectionString = model.ConnectionString;
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
                var columns = new List<HCSqlExportStreamQueryExecutorResultColumnModel>();
                foreach (var col in table.Columns.OfType<DataColumn>())
                {
                    columns.Add(new HCSqlExportStreamQueryExecutorResultColumnModel
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

                return new HCSqlExportStreamQueryExecutorResultModel()
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

            static void setCommandParameters(HCSqlExportStreamQueryExecutorQueryModel model, SqlCommand command)
            {
                command.Parameters.AddWithValue("@pageIndex", model.PageIndex);
                command.Parameters.AddWithValue("@pageSize", model.PageSize);
                command.Parameters.AddWithValue("@skipCount", model.PageIndex * model.PageSize);
                command.Parameters.AddWithValue("@takeCount", model.PageSize);
            }
        }

        private int ExecuteScalarInt(string connectionString, string query, HCSqlExportStreamQueryExecutorQueryModel model, Action<HCSqlExportStreamQueryExecutorQueryModel, SqlCommand> parameterSetter)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection);
            parameterSetter(model, command);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
