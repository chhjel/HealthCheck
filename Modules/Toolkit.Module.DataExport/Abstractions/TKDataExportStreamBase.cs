using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Models;
using QoDL.Toolkit.Module.DataExport.Models;
using QoDL.Toolkit.Module.DataExport.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Abstractions
{
    /// <summary>
    /// Stream of items that can be filtered and exported upon.
    /// <para>Base class for stronger typing.</para>
    /// </summary>
    public abstract class TKDataExportStreamBase<TItem> : ITKDataExportStream
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
        public virtual TKMemberFilterRecursive IncludedMemberFilter { get; } = new();

        /// <inheritdoc />
        public virtual bool SupportsQuery() => Method == ITKDataExportStream.QueryMethod.Queryable;

        /// <inheritdoc />
        public virtual bool AllowAnyPropertyName => false;

        /// <summary>
        /// Formatters that can be selected per column.
        /// <para>Defaults to creating from <see cref="TKDataExportService.DefaultValueFormatters"/></para>
        /// </summary>
        public virtual IEnumerable<ITKDataExportValueFormatter> ValueFormatters => TKDataExportService.DefaultValueFormatters;

        /// <summary>
        /// Defines what method to use for querying.
        /// <para><see cref="ITKDataExportStream.QueryMethod.Queryable"/> uses <see cref="GetQueryableItemsAsync"/></para>
        /// <para><see cref="ITKDataExportStream.QueryMethod.Enumerable"/> uses <see cref="GetEnumerableItemsAsync"/></para>
        /// </summary>
        public abstract ITKDataExportStream.QueryMethod Method { get; }

        /// <inheritdoc />
        public virtual Dictionary<string, object> GetAdditionalColumnValues(object item, List<string> includedProperties) => null;

        /// <inheritdoc />
        public virtual List<TKBackendInputConfig> PostprocessCustomParameterDefinitions(List<TKBackendInputConfig> customParameterDefinitions) => customParameterDefinitions;

        /// <inheritdoc />
        public virtual async Task<IQueryable> GetQueryableAsync() => await GetQueryableItemsAsync();

        /// <inheritdoc />
        public virtual async Task<ITKDataExportStream.EnumerableResult> GetEnumerableAsync(TKDataExportFilterData filter)
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
            return new ITKDataExportStream.EnumerableResult
            {
                TotalCount = result.TotalCount,
                PageItems = result.PageItems,
                Note = result.Note,
                AdditionalColumns = result.AdditionalColumns
            };
        }

        /// <summary></summary>
        internal virtual async Task<TypedEnumerableResult> GetEnumerableItemsInternalAsync(TKDataExportFilterData filter, Func<TItem, bool> predicate)
        {
            var typedFilter = new TKDataExportFilterDataTyped<TItem>
            {
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                QueryRaw = filter.QueryRaw,
                QueryPredicate = predicate,
                ParametersObj = filter.ParametersObj
            };
            return await GetEnumerableItemsAsync(typedFilter);
        }

        private static readonly TKSimpleMemoryCache _predicateCache = new();

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
            public List<TKTypeNamePair> AdditionalColumns { get; set; }
        }

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="ITKDataExportStream.QueryMethod.Queryable"/></para>
        /// </summary>
        protected virtual Task<IQueryable<TItem>> GetQueryableItemsAsync() => Task.FromResult(Enumerable.Empty<TItem>().AsQueryable());

        /// <summary>
        /// Get items to be filtered and exported.
        /// <para>Only used when <see cref="Method"/> is <see cref="ITKDataExportStream.QueryMethod.Enumerable"/></para>
        /// </summary>
        protected virtual Task<TypedEnumerableResult> GetEnumerableItemsAsync(TKDataExportFilterDataTyped<TItem> filter) => Task.FromResult(new TypedEnumerableResult());
    }

    /// <summary>
    /// Stream of items that can be filtered and exported upon.
    /// <para>Base class for use with <see cref="ITKDataExportStream.QueryMethod.Enumerable"/>.</para>
    /// </summary>
    public abstract class TKDataExportStreamBase<TItem, TParameters> : TKDataExportStreamBase<TItem>
        where TParameters : class
    {
        /// <inheritdoc />
        public override Type CustomParametersType => typeof(TParameters);

        /// <inheritdoc />
        public override ITKDataExportStream.QueryMethod Method => ITKDataExportStream.QueryMethod.Enumerable;

        /// <inheritdoc />
        internal override async Task<TypedEnumerableResult> GetEnumerableItemsInternalAsync(TKDataExportFilterData filter, Func<TItem, bool> predicate)
        {
            var typedFilter = new TKDataExportFilterDataTyped<TItem, TParameters>
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
        /// <para>Only used when <see cref="Method"/> is <see cref="ITKDataExportStream.QueryMethod.Enumerable"/></para>
        /// </summary>
        protected virtual Task<TypedEnumerableResult> GetEnumerableItemsAsync(TKDataExportFilterDataTyped<TItem, TParameters> filter) => Task.FromResult(new TypedEnumerableResult());
    }
}
