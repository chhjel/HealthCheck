using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Module.DataExport.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Module.DataExport.Abstractions.Streams.SqlExportStreamBase;

namespace HealthCheck.Module.DataExport.Abstractions.Streams
{
    public abstract class SqlExportStreamBase : HCDataExportStreamBase<TestExportItem, TestParameters>
    {
        public override bool SupportsQuery() => false;
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Enumerable;

        private readonly ISqlExportStreamQueryExecutor _queryExecutor;

        public SqlExportStreamBase(ISqlExportStreamQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        protected override async Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<TestExportItem, TestParameters> filter)
        {
            var queryModel = new SqlExportStreamQueryExecutorQueryModel
            {
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                QuerySelect = filter.Parameters.QuerySelect,
                QueryPredicate = filter.Parameters.QueryPredicate,
            };
            var queryResult = await _queryExecutor.ExecuteQueryAsync(queryModel);
            var headers = queryResult.Columns.Select((x, index) => (x.Name, index)).ToDictionaryIgnoreDuplicates(x => x.Name, x => x.index);
            var pageItems = queryResult.Rows.Select(x => new TestExportItem
            {
                Headers = headers,
                Columns = x
            }).ToList();

            var additionalColumns = queryResult.Columns.Select(x => new HCTypeNamePair { Type = x.Type, Name = x.Name }).ToList();

            return new TypedEnumerableResult
            {
                TotalCount = queryResult.TotalCount,
                PageItems = pageItems,
                Note = $"{queryResult.RecordsAffected} {"row".Pluralize(queryResult.RecordsAffected)} affected.",
                AdditionalColumns = additionalColumns
            };
        }

        public override Dictionary<string, object> GetAdditionalColumnValues(object item, List<string> includedProperties)
        {
            if (item is not TestExportItem sqlItem) return null;

            var values = new Dictionary<string, object>();
            var headers = sqlItem.Headers;
            foreach (var prop in includedProperties)
            {
                if (!headers.TryGetValue(prop, out var index)) continue;
                values[prop] = sqlItem.Columns[index];
            }
            return values;
        }

        public class TestExportItem
        {
            [JsonIgnore]
            internal Dictionary<string, int> Headers { get; set; }
            public List<object> Columns { get; set; }
        }

        public class TestParameters
        {
            public string QuerySelect { get; set; }
            public string QueryPredicate { get; set; }
        }
    }
    public interface ISqlExportStreamQueryExecutor
    {
        // TODO: allow switching connection strings here. Method to fetch connectionstring options w/ names only
        Task<SqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(SqlExportStreamQueryExecutorQueryModel model);
    }
    public class SqlExportStreamQueryExecutorQueryModel
    {
        public string QuerySelect { get; set; }
        public string QueryPredicate { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
    public class SqlExportStreamQueryExecutorResultModel
    {
        public int TotalCount { get; set; }
        public int RecordsAffected { get; set; }
        public List<SqlExportStreamQueryExecutorResultColumnModel> Columns { get; set; }
        public List<List<object>> Rows { get; set; }
    }
    public class SqlExportStreamQueryExecutorResultColumnModel
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}