using Newtonsoft.Json;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Util.Models;
using QoDL.Toolkit.Module.DataExport.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Abstractions.Streams;

/// <summary>
/// Fetches data using SQL queries.
/// </summary>
public abstract class TKSqlExportStreamBase<TParameters> : TKDataExportStreamBase<TKSqlExportStreamBase<TParameters>.SqlExportStreamData, TParameters>
    where TParameters : TKSqlExportStreamParameters
{
    /// <inheritdoc />
    public override bool SupportsQuery() => false;
    /// <inheritdoc />
    public override bool AllowAnyPropertyName { get; } = true;
    /// <inheritdoc />
    public override ITKDataExportStream.QueryMethod Method => ITKDataExportStream.QueryMethod.Enumerable;

    /// <summary>
    /// All available connectionstrings.
    /// </summary>
    protected abstract List<ConnectionStringData> ConnectionStrings { get; }

    private readonly ITKSqlExportStreamQueryExecutor _queryExecutor;

    /// <summary>
    /// Fetches data using SQL queries.
    /// </summary>
    public TKSqlExportStreamBase(ITKSqlExportStreamQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    /// <inheritdoc />
    protected override async Task<TypedEnumerableResult> GetEnumerableItemsAsync(TKDataExportFilterDataTyped<SqlExportStreamData, TParameters> filter)
    {
        var selectedConnectionString = ConnectionStrings.First(x => x.Label == filter.Parameters.ConnectionStringName);
        var queryModel = new TKSqlExportStreamQueryExecutorQueryModel
        {
            ConnectionString = selectedConnectionString.ConnectionString,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize,
            QuerySelectTotalCount = filter.Parameters.QuerySelectTotalCount,
            QuerySelectData = filter.Parameters.QuerySelectData,
            QueryPredicate = filter.Parameters.QueryPredicate,
        };
        var queryResult = await _queryExecutor.ExecuteQueryAsync(queryModel);
        var headers = queryResult.Columns.Select((x, index) => (x.Name, index)).ToDictionaryIgnoreDuplicates(x => x.Name, x => x.index);
        var pageItems = queryResult.Rows.Select(x => new SqlExportStreamData
        {
            Headers = headers,
            Columns = x
        }).ToList();

        var additionalColumns = queryResult.Columns.Select(x => new TKTypeNamePair { Type = x.Type, Name = x.Name }).ToList();

        return new TypedEnumerableResult
        {
            TotalCount = queryResult.TotalCount,
            PageItems = pageItems,
            Note = queryResult.RecordsAffected > 0 ? $"{queryResult.RecordsAffected} {"row".Pluralize(queryResult.RecordsAffected)} affected." : null,
            AdditionalColumns = additionalColumns
        };
    }

    /// <inheritdoc />
    public override Dictionary<string, object> GetAdditionalColumnValues(object item, List<string> includedProperties)
    {
        if (item is not SqlExportStreamData sqlItem) return null;

        var values = new Dictionary<string, object>();
        var headers = sqlItem.Headers;
        foreach (var prop in includedProperties)
        {
            if (!headers.TryGetValue(prop, out var index)) continue;
            values[prop] = sqlItem.Columns[index];
        }
        return values;
    }

    /// <inheritdoc />
    public override List<TKBackendInputConfig> PostprocessCustomParameterDefinitions(List<TKBackendInputConfig> customParameterDefinitions)
    {
        var config = customParameterDefinitions.First(x => x.Id == nameof(TKSqlExportStreamParameters.ConnectionStringName));
        config.Type = "Enum";
        config.PossibleValues = ConnectionStrings.Select(x => x.Label).ToList();
        return customParameterDefinitions;
    }

    /// <summary></summary>
    public class SqlExportStreamData
    {
        /// <summary></summary>
        public List<object> Columns { get; set; }

        [JsonIgnore]
        internal Dictionary<string, int> Headers { get; set; }
    }

    /// <summary></summary>
    public class ConnectionStringData
    {
        /// <summary>
        /// The value shown in the UI.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// DB connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
