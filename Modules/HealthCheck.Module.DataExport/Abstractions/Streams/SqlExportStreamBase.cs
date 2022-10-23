using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Module.DataExport.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions.Streams
{
    /// <summary>
    /// Fetches data using SQL queries.
    /// </summary>
    public abstract class HCSqlExportStreamBase<TParameters> : HCDataExportStreamBase<HCSqlExportStreamBase<TParameters>.SqlExportStreamData, TParameters>
        where TParameters : HCSqlExportStreamParameters
    {
        /// <inheritdoc />
        public override bool SupportsQuery() => false;
        /// <inheritdoc />
        public override bool AllowAnyPropertyName { get; } = true;
        /// <inheritdoc />
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Enumerable;

        /// <summary>
        /// All available connectionstrings.
        /// </summary>
        protected abstract List<ConnectionStringData> ConnectionStrings { get; }

        private readonly IHCSqlExportStreamQueryExecutor _queryExecutor;

        /// <summary>
        /// Fetches data using SQL queries.
        /// </summary>
        public HCSqlExportStreamBase(IHCSqlExportStreamQueryExecutor queryExecutor)
        {
            _queryExecutor = queryExecutor;
        }

        /// <inheritdoc />
        protected override async Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<SqlExportStreamData, TParameters> filter)
        {
            var queryModel = new HCSqlExportStreamQueryExecutorQueryModel
            {
                ConnectionString =  @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=FeatureStudioDevNetFramework;Integrated Security=True;",
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

            var additionalColumns = queryResult.Columns.Select(x => new HCTypeNamePair { Type = x.Type, Name = x.Name }).ToList();

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
}
