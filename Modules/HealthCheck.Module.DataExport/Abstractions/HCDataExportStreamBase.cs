using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
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

        /// <inheritdoc />
        public virtual Type CustomParametersType { get; }

        /// <inheritdoc />
        public bool SupportsQuery => Method != IHCDataExportStream.QueryMethod.EnumerableWithCustomFilter;

        /// <inheritdoc />
        public virtual IEnumerable<IHCDataExportValueFormatter> ValueFormatters { get; } = new List<IHCDataExportValueFormatter>();

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

        /// <inheritdoc />
        public virtual Task<IHCDataExportStream.EnumerableResult> GetEnumerableWithCustomFilterAsync(int pageIndex, int pageSize, object parameters)
            => throw new HCException($"To use {nameof(IHCDataExportStream.QueryMethod.EnumerableWithCustomFilter)} you must either override {nameof(GetEnumerableWithCustomFilterAsync)} or use the {nameof(HCDataExportStreamBase<object, object>)} base class.");

        /// <inheritdoc />
        public virtual object DefaultFormatValue(string propertyName, Type propertyType, object value)
        {
            lock(_formatValueMethodCache)
            {
                if (!_formatValueMethodCache.ContainsKey(propertyType))
                {
                    _formatValueMethodCache[propertyType] = GetType()
                        .GetMethod(nameof(DefaultFormatValue), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(propertyType);
                }
                var method = _formatValueMethodCache[propertyType];
                return method.Invoke(this, new object[] { propertyName, value });
            }
        }
        private static readonly Dictionary<Type, MethodInfo> _formatValueMethodCache = new();

        /// <summary>
        /// Strongly typed version of <see cref="DefaultFormatValue(string, Type, object)"/>.
        /// </summary>
        /// <param name="propertyName">Dotted path to the member.</param>
        /// <param name="value">Value of the property.</param>
        protected virtual object DefaultFormatValue<T>(string propertyName, T value) => value;

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

    /// <summary>
    /// Stream of items that can be filtered and exported upon.
    /// <para>Base class for use with <see cref="IHCDataExportStream.QueryMethod.EnumerableWithCustomFilter"/>.</para>
    /// </summary>
    public abstract class HCDataExportStreamBase<TItem, TParameters> : HCDataExportStreamBase<TItem>
        where TParameters : class
    {
        /// <inheritdoc />
        public override Type CustomParametersType => typeof(TParameters);

        /// <inheritdoc />
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.EnumerableWithCustomFilter;

        /// <inheritdoc />
        public override async Task<IHCDataExportStream.EnumerableResult> GetEnumerableWithCustomFilterAsync(int pageIndex, int pageSize, object parameters)
        {
            var result = await GetEnumerableItemsWithCustomFilterAsync(pageIndex, pageSize, parameters as TParameters);
            return new IHCDataExportStream.EnumerableResult
            {
                TotalCount = result.TotalCount,
                PageItems = result.PageItems
            };
        }

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="HCDataExportStreamBase{TItem}.Method"/> is <see cref="IHCDataExportStream.QueryMethod.EnumerableWithCustomFilter"/></para>
        /// </summary>
        protected abstract Task<TypedEnumerableResult> GetEnumerableItemsWithCustomFilterAsync(int pageIndex, int pageSize, TParameters parameters);
    }
}
