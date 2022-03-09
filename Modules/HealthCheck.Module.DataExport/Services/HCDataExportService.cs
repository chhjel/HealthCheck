using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Formatters;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Services
{
    /// <summary>
    /// Handles export stream data.
    /// </summary>
    public class HCDataExportService : IHCDataExportService
    {
        private readonly IEnumerable<IHCDataExportStream> _streams;
        private static readonly Dictionary<string, HCDataExportStreamItemDefinition> _streamItemTypeDefCache = new();

        /// <summary>
        /// Handles export stream data.
        /// </summary>
        public HCDataExportService(IEnumerable<IHCDataExportStream> streams)
        {
            _streams = streams;
        }

        /// <summary>
        /// Includes built in DateTime and Enumerable value formatters.
        /// </summary>
        public static IEnumerable<IHCDataExportValueFormatter> DefaultValueFormatters
            => new IHCDataExportValueFormatter[] {
                new HCDataExportDateTimeValueFormatter(),
                new HCDataExportEnumerableValueFormatter()
            };

        /// <inheritdoc />
        public IEnumerable<IHCDataExportStream> GetStreams() => _streams;

        /// <inheritdoc />
        public HCDataExportStreamItemDefinition GetStreamItemDefinition(string streamId, Type itemType, IEnumerable<IHCDataExportValueFormatter> valueFormatters)
        {
            lock (_streamItemTypeDefCache)
            {
                var cacheKey = $"{streamId}::{itemType.FullName}";
                if (_streamItemTypeDefCache.ContainsKey(cacheKey))
                {
                    return _streamItemTypeDefCache[cacheKey];
                }

                var model = new HCDataExportStreamItemDefinition
                {
                    StreamId = streamId,
                    Name = itemType.Name.SpacifySentence()
                };

                var stream = GetStreamById(streamId);
                int maxDepth = stream.MaxMemberDiscoveryDepth ?? 4;
                var memberFilter = stream.IncludedMemberFilter ?? new HCMemberFilterRecursive();
                memberFilter.PropertyFilter ??= new HCPropertyFilter();
                memberFilter.TypeFilter ??= new HCTypeFilter();

                model.Members = HCReflectionUtils.GetTypeMembersRecursive(itemType, maxDepth, stream.IncludedMemberFilter)
                    .Select(x => new HCDataExportStreamItemDefinitionMember
                    {
                        Name = x.Name,
                        Type = x.Type,
                        FormatterIds = valueFormatters
                            .Where(f => f.SupportedTypes?.Any(t => t.IsAssignableFromIncludingNullable(x.Type)) == true
                                && f.NotSupportedTypes?.Contains(x.Type) != true)
                            .Select(x => x.GetType().FullName)
                    })
                    .ToList();

                _streamItemTypeDefCache[cacheKey] = model;
                return model;
            }
        }

        /// <inheritdoc />
        public async Task<HCDataExportQueryResponse> QueryAsync(HCDataExportQueryRequest request)
        {
            var stream = GetStreamById(request.StreamId);
            if (stream == null)
            {
                return new HCDataExportQueryResponse();
            }

            var totalCount = 0;
            object[] pageItems = Array.Empty<object>();

            if (stream.Method == IHCDataExportStream.QueryMethod.Queryable)
            {
                var queryable = await stream.GetQueryableAsync();
                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    queryable = queryable.Where(request.Query);
                }
                var matches = queryable
                    .Cast<object>()
                    .ToArray();
                totalCount = matches.Length;

                pageItems = matches
                    .Skip(request.PageIndex * request.PageSize)
                    .Take(request.PageSize)
                    .Cast<object>()
                    .ToArray();
            }
            else if (stream.Method == IHCDataExportStream.QueryMethod.Enumerable)
            {
                var enumerableResult = await stream.GetEnumerableAsync(request.PageIndex, request.PageSize, request.Query);
                pageItems = enumerableResult?.PageItems?.Cast<object>()?.ToArray() ?? Array.Empty<object>();
                totalCount = enumerableResult?.TotalCount ?? 0;
            }
            else if (stream.Method == IHCDataExportStream.QueryMethod.EnumerableWithCustomFilter)
            {
                object parametersObject = stream.CustomParametersType == null ? null : HCValueConversionUtils.ConvertInputModel(stream.CustomParametersType, request.CustomParameters);
                var enumerableResult = await stream.GetEnumerableWithCustomFilterAsync(request.PageIndex, request.PageSize, parametersObject);
                pageItems = enumerableResult?.PageItems?.Cast<object>()?.ToArray() ?? Array.Empty<object>();
                totalCount = enumerableResult?.TotalCount ?? 0;
            }

            var formatters = stream.ValueFormatters?.ToDictionaryIgnoreDuplicates(x => x.GetType().FullName, x => x);
            var resultItems = pageItems
                .Where(x => x != null)
                .Select(x => CreateResultItem(x, stream, request.IncludedProperties, request.CustomColumns, request.ValueFormatterConfigs, formatters))
                .ToArray();

            var result = new HCDataExportQueryResponse
            {
                Items = resultItems,
                TotalCount = totalCount
            };
            return result;
        }

        private Dictionary<string, object> CreateResultItem(object item, IHCDataExportStream stream,
            List<string> includedProperties, Dictionary<string, string> customColumns, Dictionary<string, HCDataExportValueFormatterConfig> valueFormatterConfigs, Dictionary<string, IHCDataExportValueFormatter> formatters)
        {
            var streamId = stream.GetType().FullName;
            var itemType = item.GetType();
            var valueFormatters = (stream.ValueFormatters ?? Array.Empty<IHCDataExportValueFormatter>());
            var itemDef = GetStreamItemDefinition(streamId, itemType, valueFormatters);

            var dict = new Dictionary<string, object>();
            var allowedIncludedProperties = itemDef.Members.Where(x => includedProperties.Count == 0 || includedProperties.Any(m => m == x.Name));
            foreach (var prop in allowedIncludedProperties)
            {
                var value = prop.GetValue(item);

                var customFormatterConfig = valueFormatterConfigs?.ContainsKey(prop.Name) == true ? valueFormatterConfigs[prop.Name] : null;
                var customFormatter = (customFormatterConfig?.FormatterId != null && formatters.ContainsKey(customFormatterConfig.FormatterId))
                    ? formatters[customFormatterConfig.FormatterId] : null;

                // Custom format
                if (customFormatter != null)
                {
                    // Only build parameter object once
                    if (customFormatterConfig.Parameters == null)
                    {
                        customFormatterConfig.Parameters = HCValueConversionUtils.ConvertInputModel(customFormatter.CustomParametersType, customFormatterConfig.CustomParameters)
                            ?? Activator.CreateInstance(customFormatter.CustomParametersType);
                    }

                    value = customFormatter.FormatValue(prop.Name, prop.Type, value, customFormatterConfig.Parameters);
                }
                // Default format
                else
                {
                    value = stream.DefaultFormatValue(prop.Name, prop.Type, value);
                }

                dict[prop.Name] = value;
            }

            // Custom columns
            foreach(var kvp in customColumns)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                foreach (var member in itemDef.Members)
                {
                    var placeholder = $"{{{member.Name}}}";
                    if (value.Contains(placeholder))
                    {
                        var memberValue = member.GetValue(item);
                        memberValue = stream.DefaultFormatValue(member.Name, member.Type, memberValue);
                        value = value.Replace(placeholder, SerializeOrStringifyValue(memberValue));
                    }
                }

                dict[key] = value;
            }

            return dict;
        }

        internal static string SerializeOrStringifyValue(object val, Dictionary<Type, bool> cache = null)
        {
            var shouldSerialize = ShouldSerializeValue(val, cache);
            return shouldSerialize ? HCGlobalConfig.Serializer.Serialize(val, pretty: false) : val?.ToString();
        }

        internal static Dictionary<Type, bool> _serializeStringifiyCache = new();
        internal static bool ShouldSerializeValue(object val, Dictionary<Type, bool> cache = null)
        {
            cache ??= _serializeStringifiyCache;

            var type = val?.GetType();
            if (type == null) return false;
            else if (cache?.ContainsKey(type) == true) return cache[type];

            var toStringMethod = type.GetMethods()?.FirstOrDefault(x => x.Name == nameof(object.ToString));
            return toStringMethod?.DeclaringType == typeof(object);
        }

        private IHCDataExportStream GetStreamById(string streamId)
            => _streams.FirstOrDefault(x => x.GetType().FullName == streamId);
    }
}
