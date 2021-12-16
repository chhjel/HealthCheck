using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Stream of items that can be filtered and exported upon.
    /// <para>Base class for stronger typing.</para>
    /// </summary>
    public abstract class HCDataExportStreamBase<TItem> : IHCDataExportStream
    {
        /// <inheritdoc />
        public Type ItemType => typeof(TItem);

        /// <inheritdoc />
        public abstract string StreamDisplayName { get; }

        /// <inheritdoc />
        public abstract string StreamDescription { get; }

        /// <inheritdoc />
        public virtual string StreamGroupName { get; }

        /// <inheritdoc />
        public virtual object AllowedAccessRoles { get; }

        /// <inheritdoc />
        public virtual List<string> Categories { get; }

        /// <inheritdoc />
        public abstract int ExportBatchSize { get; }

        /// <summary>
        /// Defines what method to use for querying.
        /// <para><see cref="IHCDataExportStream.QueryMethod.Queryable"/> uses <see cref="GetQueryableItemsAsync()"/></para>
        /// <para><see cref="IHCDataExportStream.QueryMethod.QueryableManuallyPaged"/> uses <see cref="GetQueryableItemsManuallyPagedAsync"/></para>
        /// <para><see cref="IHCDataExportStream.QueryMethod.Enumerable"/> uses <see cref="GetEnumerableItemsAsync"/></para>
        /// </summary>
        public abstract IHCDataExportStream.QueryMethod Method { get; }

        /// <inheritdoc />
        public virtual async Task<IQueryable> GetQueryableAsync()
            => await GetQueryableItemsAsync();

        /// <inheritdoc />
        public async Task<IHCDataExportStream.QueryableResult> GetQueryableManuallyPagedAsync(int pageIndex, int pageSize)
        {
            var result = await GetQueryableItemsManuallyPagedAsync(pageIndex, pageSize);
            return new IHCDataExportStream.QueryableResult
            {
                TotalCount = result.TotalCount,
                PageItems = result.PageItems
            };
        }

        /// <inheritdoc />
        public virtual async Task<IHCDataExportStream.EnumerableResult> GetEnumerableAsync(int pageIndex, int pageSize, string query)
        {
            // Cache for a bit since it takes some time on every page when exporting
            var predicate = _predicateCache.GetValue<Func<TItem, bool>>(query, null);
            if (predicate == null)
            {
                var expression = DynamicExpressionParser.ParseLambda<TItem, bool>(new ParsingConfig(), true, query);
                predicate = expression.Compile();
                _predicateCache.SetValue(query, predicate, TimeSpan.FromMinutes(1));
            }

            var result = await GetEnumerableItemsAsync(pageIndex, pageSize, predicate);
            return new IHCDataExportStream.EnumerableResult
            {
                TotalCount = result.TotalCount,
                PageItems = result.PageItems
            };
        }
        private static readonly SimpleMemoryCache _predicateCache = new();

        /// <summary>
        /// Result from <see cref="GetEnumerableItemsAsync"/>
        /// </summary>
        public class TypedEnumerableResult
        {
            /// <summary>
            /// Matching items for the given page.
            /// </summary>
            public IEnumerable<TItem> PageItems { get; set; } = Enumerable.Empty<TItem>();

            /// <summary>
            /// Total match count.
            /// </summary>
            public int TotalCount { get; set; }
        }

        /// <summary>
        /// Result from <see cref="GetQueryableItemsManuallyPagedAsync(int, int)"/>
        /// </summary>
        public class TypedQueryableResult
        {
            /// <summary>
            /// Matching items for the given page.
            /// </summary>
            public IQueryable<TItem> PageItems { get; set; } = Enumerable.Empty<TItem>().AsQueryable();

            /// <summary>
            /// Total match count.
            /// </summary>
            public int TotalCount { get; set; }
        }

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="IHCDataExportStream.QueryMethod.Queryable"/></para>
        /// </summary>
        protected virtual Task<IQueryable<TItem>> GetQueryableItemsAsync() => Task.FromResult(Enumerable.Empty<TItem>().AsQueryable());

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="IHCDataExportStream.QueryMethod.QueryableManuallyPaged"/></para>
        /// </summary>
        protected virtual Task<TypedQueryableResult> GetQueryableItemsManuallyPagedAsync(int pageIndex, int pageSize) => Task.FromResult(new TypedQueryableResult());

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="IHCDataExportStream.QueryMethod.Enumerable"/></para>
        /// </summary>
        protected virtual Task<TypedEnumerableResult> GetEnumerableItemsAsync(int pageIndex, int pageSize, Func<TItem, bool> predicate) => Task.FromResult(new TypedEnumerableResult());
    }
}
