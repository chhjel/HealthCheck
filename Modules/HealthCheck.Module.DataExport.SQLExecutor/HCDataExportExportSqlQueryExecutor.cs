using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
            var totalCountResult = await GetTotalResultCountAsync(connectionString, totalCountQuery, model, setCommandParameters);
            var totalCount = totalCountResult.Item1;
            var totalCountRowsAffected = totalCountResult.Item2;

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

                var rowsAffected = ((reader.RecordsAffected < 0) ? 0 : reader.RecordsAffected)
                    + ((totalCountRowsAffected < 0) ? 0 : totalCountRowsAffected);

                return new HCSqlExportStreamQueryExecutorResultModel()
                {
                    TotalCount = totalCount,
                    RecordsAffected = rowsAffected,
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

        private async Task<(int, int)> GetTotalResultCountAsync(string connectionString, string query, HCSqlExportStreamQueryExecutorQueryModel model, Action<HCSqlExportStreamQueryExecutorQueryModel, SqlCommand> parameterSetter)
        {
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(query, connection);
            parameterSetter(model, command);
            connection.Open();

            var reader = await command.ExecuteReaderAsync();
            var totalCount = 0;
            if (reader.Read())
            {
                totalCount = reader.GetInt32(0);
            }
            var rowsAffected = reader.RecordsAffected;

            return (totalCount, rowsAffected);
        }
    }
}
