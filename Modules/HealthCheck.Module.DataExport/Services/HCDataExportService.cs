using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util;
using HealthCheck.Module.DataExport.Abstractions;
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

        /// <inheritdoc />
        public IEnumerable<IHCDataExportStream> GetStreams() => _streams;

        /// <inheritdoc />
        public HCDataExportStreamItemDefinition GetStreamItemDefinition(string streamId, Type itemType)
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
                var ignoredTypes = new HashSet<Type>(stream.IgnoredMemberTypes ?? Enumerable.Empty<Type>());
                var ignoredPrefixes = stream.IgnoredMemberPathPrefixes?.ToArray() ?? Array.Empty<string>();

                model.Members = ReflectionUtils.GetTypeMembersRecursive(itemType, maxLevels: maxDepth, ignoredTypes: ignoredTypes)
                    .Where(x => !ignoredPrefixes.Any(p => x.Name.StartsWith(p)))
                    .Select(x => new HCDataExportStreamItemDefinitionMember
                    {
                        Name = x.Name,
                        Type = x.Type
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
            if (stream == null
                || (stream.SupportsQuery && string.IsNullOrWhiteSpace(request.Query)))
            {
                return new HCDataExportQueryResponse();
            }

            var totalCount = 0;
            object[] pageItems = Array.Empty<object>();

            if (stream.Method == IHCDataExportStream.QueryMethod.Queryable)
            {
                var queryable = await stream.GetQueryableAsync();
                var matches = queryable
                    .Where(request.Query)
                    .Cast<object>()
                    .ToArray();
                totalCount = matches.Length;

                pageItems = matches
                    .Skip(request.PageIndex * request.PageSize)
                    .Take(request.PageSize)
                    .Cast<object>()
                    .ToArray();
            }
            else if (stream.Method == IHCDataExportStream.QueryMethod.QueryableManuallyPaged)
            {
                var queryableResult = await stream.GetQueryableManuallyPagedAsync(request.PageIndex, request.PageSize);
                pageItems = queryableResult?.PageItems?.Cast<object>()?.ToArray() ?? Array.Empty<object>();
                totalCount = queryableResult?.TotalCount ?? 0;
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
            var serializeStringifiyCache = new Dictionary<Type, bool>();
            var resultItems = pageItems
                .Where(x => x != null)
                .Select(x => CreateResultItem(x, stream, request.IncludedProperties, request.CustomColumns, request.ValueFormatterConfigs, formatters, serializeStringifiyCache))
                .ToArray();

            var result = new HCDataExportQueryResponse
            {
                Items = resultItems,
                TotalCount = totalCount
            };
            return result;
        }

        private Dictionary<string, object> CreateResultItem(object item, IHCDataExportStream stream,
            List<string> includedProperties, Dictionary<string, string> customColumns, Dictionary<string, HCDataExportValueFormatterConfig> valueFormatterConfigs, Dictionary<string, IHCDataExportValueFormatter> formatters, Dictionary<Type, bool> serializeStringifiyCache)
        {
            var streamId = stream.GetType().FullName;
            var itemType = item.GetType();
            var itemDef = GetStreamItemDefinition(streamId, itemType);

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
                        value = value.Replace(placeholder, SerializeOrStringifyValue(memberValue, serializeStringifiyCache));
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

        internal static bool ShouldSerializeValue(object val, Dictionary<Type, bool> cache = null)
        {
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
