using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.SQLExecutor;

/// <summary></summary>
public class TKDataExportExportSqlQueryExecutor : ITKSqlExportStreamQueryExecutor
{
    /// <summary>
    /// Throws an exception if the incoming queries seems to contain something that would result in a change of data.
    /// <para>If set to false, updates/inserts/drops etc won't be attempted stopped.</para>
    /// <para>For use when a readonly connectionstring is not available.</para>
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool TryPreventChanges { get; set; } = true;

    /// <summary></summary>
    public async Task<TKSqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(TKSqlExportStreamQueryExecutorQueryModel model)
    {
        string connectionString = model.ConnectionString;
        var totalCountQuery = model.QuerySelectTotalCount.Replace("[PREDICATE]", model.QueryPredicate);
        var dataQuery = model.QuerySelectData.Replace("[PREDICATE]", model.QueryPredicate);

        if (TryPreventChanges)
        {
            var totalCountQueryValidation = TKSQLUtils.TryCheckQueryForThingsThatCauseChanges(totalCountQuery);
            if (!totalCountQueryValidation.Valid) throw new SqlValidationException(totalCountQueryValidation.InvalidReason);
            var dataQueryValidation = TKSQLUtils.TryCheckQueryForThingsThatCauseChanges(dataQuery);
            if (!dataQueryValidation.Valid) throw new SqlValidationException(dataQueryValidation.InvalidReason);
        }

        var totalCountResult = await GetTotalResultCountAsync(connectionString, totalCountQuery, model, setCommandParameters);
        var totalCount = totalCountResult.Item1;
        var totalCountRowsAffected = totalCountResult.Item2;

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
            var columns = new List<TKSqlExportStreamQueryExecutorResultColumnModel>();
            foreach (var col in table.Columns.OfType<DataColumn>())
            {
                columns.Add(new TKSqlExportStreamQueryExecutorResultColumnModel
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

            return new TKSqlExportStreamQueryExecutorResultModel()
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

        static void setCommandParameters(TKSqlExportStreamQueryExecutorQueryModel model, SqlCommand command)
        {
            command.Parameters.AddWithValue("@pageIndex", model.PageIndex);
            command.Parameters.AddWithValue("@pageSize", model.PageSize);
            command.Parameters.AddWithValue("@skipCount", model.PageIndex * model.PageSize);
            command.Parameters.AddWithValue("@takeCount", model.PageSize);
        }
    }

    private async Task<(int, int)> GetTotalResultCountAsync(string connectionString, string query, TKSqlExportStreamQueryExecutorQueryModel model, Action<TKSqlExportStreamQueryExecutorQueryModel, SqlCommand> parameterSetter)
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

    /// <summary></summary>
    [Serializable]
    public class SqlValidationException : Exception
    {
        /// <summary></summary>
        public SqlValidationException() { }
        /// <summary></summary>
        public SqlValidationException(string message) : base(message) { }
        /// <summary></summary>
        public SqlValidationException(string message, Exception inner) : base(message, inner) { }
        /// <summary></summary>
        protected SqlValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
