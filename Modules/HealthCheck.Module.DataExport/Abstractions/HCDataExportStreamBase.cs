using HealthCheck.Core.Models;
using HealthCheck.Core.Util;
using HealthCheck.Module.DataExport.Models;
using HealthCheck.Module.DataExport.Services;
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

        /// <summary>
        /// Max depth to search for members recursively.
        /// <para>Defaults to 4.</para>
        /// </summary>
        public virtual int? MaxMemberDiscoveryDepth { get; } = 4;

        /// <inheritdoc />
        public virtual HCMemberFilterRecursive IncludedMemberFilter { get; } = new();

        /// <inheritdoc />
        public virtual bool SupportsQuery() => Method == IHCDataExportStream.QueryMethod.Queryable;

        /// <inheritdoc />
        public virtual bool AllowAnyPropertyName => false;

        /// <summary>
        /// Formatters that can be selected per column.
        /// <para>Defaults to creating from <see cref="HCDataExportService.DefaultValueFormatters"/></para>
        /// </summary>
        public virtual IEnumerable<IHCDataExportValueFormatter> ValueFormatters => HCDataExportService.DefaultValueFormatters;

        /// <summary>
        /// Defines what method to use for querying.
        /// <para><see cref="IHCDataExportStream.QueryMethod.Queryable"/> uses <see cref="GetQueryableItemsAsync"/></para>
        /// <para><see cref="IHCDataExportStream.QueryMethod.Enumerable"/> uses <see cref="GetEnumerableItemsAsync"/></para>
        /// </summary>
        public abstract IHCDataExportStream.QueryMethod Method { get; }

        /// <inheritdoc />
        public virtual Dictionary<string, object> GetAdditionalColumnValues(object item, List<string> includedProperties) => null;

        /// <inheritdoc />
        public virtual async Task<IQueryable> GetQueryableAsync() => await GetQueryableItemsAsync();

        /// <inheritdoc />
        public virtual async Task<IHCDataExportStream.EnumerableResult> GetEnumerableAsync(HCDataExportFilterData filter)
        {
            var query = filter.QueryRaw;

            // Cache for a bit since it takes some time on every page when exporting
            var predicate = _predicateCache.GetValue<Func<TItem, bool>>(query, null);
            if (predicate == null)
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    predicate = x => true;
                }
                else
                {
                    var expression = DynamicExpressionParser.ParseLambda<TItem, bool>(new ParsingConfig(), true, query);
                    predicate = expression.Compile();
                }
                _predicateCache.SetValue(query, predicate, TimeSpan.FromMinutes(1));
            }

            var result = await GetEnumerableItemsInternalAsync(filter, predicate);
            return new IHCDataExportStream.EnumerableResult
            {
                TotalCount = result.TotalCount,
                PageItems = result.PageItems,
                Note = result.Note,
                AdditionalColumns = result.AdditionalColumns
            };
        }

        /// <summary></summary>
        internal virtual async Task<TypedEnumerableResult> GetEnumerableItemsInternalAsync(HCDataExportFilterData filter, Func<TItem, bool> predicate)
        {
            var typedFilter = new HCDataExportFilterDataTyped<TItem>
            {
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                QueryRaw = filter.QueryRaw,
                QueryPredicate = predicate,
                ParametersObj = filter.ParametersObj
            };
            return await GetEnumerableItemsAsync(typedFilter);
        }

        private static readonly HCSimpleMemoryCache _predicateCache = new();

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
                if (!method.GetGenericArguments()[0].GetType().IsInstanceOfType(value)) return value;
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

            /// <summary>
            /// Optional note.
            /// </summary>
            public string Note { get; set; }

            /// <summary>
            /// Optionally force result headers.
            /// </summary>
            public List<HCTypeNamePair> AdditionalColumns { get; set; }
        }

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="IHCDataExportStream.QueryMethod.Queryable"/></para>
        /// </summary>
        protected virtual Task<IQueryable<TItem>> GetQueryableItemsAsync() => Task.FromResult(Enumerable.Empty<TItem>().AsQueryable());

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="IHCDataExportStream.QueryMethod.Enumerable"/></para>
        /// </summary>
        protected virtual Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<TItem> filter) => Task.FromResult(new TypedEnumerableResult());
    }

    /// <summary>
    /// Stream of items that can be filtered and exported upon.
    /// <para>Base class for use with <see cref="IHCDataExportStream.QueryMethod.Enumerable"/>.</para>
    /// </summary>
    public abstract class HCDataExportStreamBase<TItem, TParameters> : HCDataExportStreamBase<TItem>
        where TParameters : class
    {
        /// <inheritdoc />
        public override Type CustomParametersType => typeof(TParameters);

        /// <inheritdoc />
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Enumerable;

        /// <inheritdoc />
        internal override async Task<TypedEnumerableResult> GetEnumerableItemsInternalAsync(HCDataExportFilterData filter, Func<TItem, bool> predicate)
        {
            var typedFilter = new HCDataExportFilterDataTyped<TItem, TParameters>
            {
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                QueryRaw = filter.QueryRaw,
                QueryPredicate = predicate,
                ParametersObj = filter.ParametersObj,
                Parameters = filter.ParametersObj as TParameters
            };
            return await GetEnumerableItemsAsync(typedFilter);
        }

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="IHCDataExportStream.QueryMethod.Enumerable"/></para>
        /// </summary>
        protected virtual Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<TItem, TParameters> filter) => Task.FromResult(new TypedEnumerableResult());
    }
}
