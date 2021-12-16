using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Represents a stream of items that can be filtered and exported upon.
    /// </summary>
    public interface IHCDataExportStream
    {
        /// <summary>
        /// Name of the stream.
        /// </summary>
        string StreamDisplayName { get; }

        /// <summary>
        /// Optional description of the stream. Supports HTML.
        /// </summary>
        string StreamDescription { get; }

        /// <summary>
        /// Name of the stream group if any.
        /// </summary>
        string StreamGroupName { get; }

        /// <summary>
        /// Optional access roles that can access this stream.
        /// <para>Must be a flags enum of the same type as the one used on the healthcheck controller.</para>
        /// </summary>
        object AllowedAccessRoles { get; }

        /// <summary>
        /// Optional categories this stream belongs to.
        /// <para>Can be used for more granular access configuration.</para>
        /// </summary>
        List<string> Categories { get; }

        /// <summary>
        /// Type of items returned.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Number of items to fetch per batch when exporting.
        /// </summary>
        int ExportBatchSize { get; }

        /// <summary>
        /// Defines what method to use for querying.
        /// </summary>
        public QueryMethod Method { get; }

        /// <summary>
        /// Get items to be filtered and exported. Invoked when <see cref="Method"/> is <see cref="QueryMethod.Queryable"/>.
        /// <para>For use when you have an IQueryable source.</para>
        /// </summary>
        Task<IQueryable> GetQueryableAsync();

        /// <summary>
        /// Get items to be filtered and exported. Invoked when <see cref="Method"/> is <see cref="QueryMethod.QueryableManuallyPaged"/>.
        /// <para>For use when you have an IQueryable but need to handle pagination manually.</para>
        /// </summary>
        Task<QueryableResult> GetQueryableManuallyPagedAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Get items to be filtered and exported. Invoked when <see cref="Method"/> is <see cref="QueryMethod.Enumerable"/>.
        /// </summary>
        /// <param name="pageIndex">Index of the page to fetch.</param>
        /// <param name="pageSize">Size of the page to fetch.</param>
        /// <param name="query">The raw query input from frontend.</param>
        Task<EnumerableResult> GetEnumerableAsync(int pageIndex, int pageSize, string query);

        /// <summary>
        /// Result from <see cref="GetEnumerableAsync"/>
        /// </summary>
        public class EnumerableResult
        {
            /// <summary>
            /// Matching items for the given page.
            /// </summary>
            public System.Collections.IEnumerable PageItems { get; set; }

            /// <summary>
            /// Total match count.
            /// </summary>
            public int TotalCount { get; set; }
        }

        /// <summary>
        /// Result from <see cref="GetQueryableManuallyPagedAsync"/>
        /// </summary>
        public class QueryableResult
        {
            /// <summary>
            /// Matching items for the given page.
            /// </summary>
            public System.Linq.IQueryable PageItems { get; set; }

            /// <summary>
            /// Total match count.
            /// </summary>
            public int TotalCount { get; set; }
        }

        /// <summary>
        /// What method to use for querying.
        /// </summary>
        public enum QueryMethod
        {
            /// <summary>
            /// Use the get queryable method without any parameters.
            /// </summary>
            Queryable,

            /// <summary>
            /// Use the get queryable method with manual paging.
            /// </summary>
            QueryableManuallyPaged,

            /// <summary>
            /// Use the get enumerable method with query parameter.
            /// </summary>
            Enumerable
        }
    }
}
