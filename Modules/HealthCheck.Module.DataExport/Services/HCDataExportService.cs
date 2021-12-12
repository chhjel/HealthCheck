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

                model.Members = ReflectionUtils.GetTypeMembersRecursive(itemType)
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
            var stream = GetStreamById(request);
            if (stream == null || string.IsNullOrWhiteSpace(request.Query))
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
            else if (stream.Method == IHCDataExportStream.QueryMethod.Enumerable)
            {
                var enumerableResult = await stream.GetEnumerableAsync(request.PageIndex, request.PageSize, request.Query);
                pageItems = enumerableResult?.PageItems?.Cast<object>()?.ToArray() ?? Array.Empty<object>();
                totalCount = enumerableResult?.TotalCount ?? 0;
            }

            var resultItems = pageItems
                .Where(x => x != null)
                .Select(x => CreateResultItem(x, stream.GetType().FullName, request.IncludedProperties))
                .ToArray();

            var result = new HCDataExportQueryResponse
            {
                Items = resultItems,
                TotalCount = totalCount
            };
            return result;
        }

        private Dictionary<string, object> CreateResultItem(object item, string streamId, List<string> includedProperties)
        {
            var itemType = item.GetType();
            var itemDef = GetStreamItemDefinition(streamId, itemType);

            var dict = new Dictionary<string, object>();
            var allowedIncludedProperties = itemDef.Members.Where(x => includedProperties.Count == 0 || includedProperties.Any(m => m == x.Name));
            foreach (var prop in allowedIncludedProperties)
            {
                dict[prop.Name] = prop.GetValue(item);
            }

            return dict;
        }

        private IHCDataExportStream GetStreamById(HCDataExportQueryRequest request)
            => _streams.FirstOrDefault(x => x.GetType().FullName == request.StreamId);
    }
}
