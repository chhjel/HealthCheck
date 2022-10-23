using HealthCheck.Core.Attributes;
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
        public override bool AllowAnyPropertyName { get; } = true;
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
                QuerySelectTotalCount = filter.Parameters.QuerySelectTotalCount,
                QuerySelectData = filter.Parameters.QuerySelectData,
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
                Note = queryResult.RecordsAffected > 0 ? $"{queryResult.RecordsAffected} {"row".Pluralize(queryResult.RecordsAffected)} affected." : null,
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
            [HCCustomProperty(UIHints = HCUIHint.CodeArea | HCUIHint.NotNull | HCUIHint.FullWidth, CodeLanguage = "sql")]
            public string QuerySelectTotalCount { get; set; } = "SELECT COUNT(*)\n[PREDICATE]";
            [HCCustomProperty(UIHints = HCUIHint.CodeArea | HCUIHint.NotNull | HCUIHint.FullWidth, CodeLanguage = "sql")]
            public string QuerySelectData { get; set; } = "SELECT *\n[PREDICATE]\nORDER BY \nOFFSET @skipCount ROWS\nFETCH NEXT @takeCount ROWS ONLY";
            [HCCustomProperty(UIHints = HCUIHint.CodeArea | HCUIHint.NotNull | HCUIHint.FullWidth, CodeLanguage = "sql",
                Description = "Available parameters: @pageIndex, @pageSize, @skipCount, @takeCount")]
            public string QueryPredicate { get; set; } = "FROM ";
        }
    }
    public interface ISqlExportStreamQueryExecutor
    {
        // TODO: allow switching connection strings here. Method to fetch connectionstring options w/ names only
        // TODO: allow custom parameters defined in frontend => use for parameterized query presets
        // FROM OXX_FStudio_feature_variants

        Task<SqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(SqlExportStreamQueryExecutorQueryModel model);
    }
    public class SqlExportStreamQueryExecutorQueryModel
    {
        public string QuerySelectTotalCount { get; set; }
        public string QuerySelectData { get; set; }
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