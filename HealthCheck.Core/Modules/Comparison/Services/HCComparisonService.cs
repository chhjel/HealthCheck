﻿using HealthCheck.Core.Modules.Comparison.Abstractions;
using HealthCheck.Core.Modules.Comparison.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Comparison.Services
{
    /// <summary>
    /// Handles comparison of data.
    /// </summary>
    public class HCComparisonService : IHCComparisonService
    {
        private readonly IEnumerable<IHCComparisonTypeHandler> _typeHandlers;
        private readonly IEnumerable<IHCComparisonDiffer> _differs;

        /// <summary>
        /// Handles comparison of data.
        /// </summary>
        public HCComparisonService(IEnumerable<IHCComparisonTypeHandler> typeHandlers, IEnumerable<IHCComparisonDiffer> differs)
        {
            _typeHandlers = typeHandlers;
            _differs = differs;
        }

        /// <inheritdoc />
        public List<HCComparisonTypeDefinition> GetComparisonTypeDefinitions()
        {
            return _typeHandlers
                .Select(x => new HCComparisonTypeDefinition
                {
                    Id = x.GetType().Name,
                    Name = x.Name,
                    Description = x.Description,
                    FindInstanceDescription = x.FindInstanceDescription,
                    FindInstanceSearchPlaceholder = x.FindInstanceSearchPlaceholder
                })
                .ToList();
        }

        /// <inheritdoc />
        public Dictionary<string, List<HCComparisonDifferDefinition>> GetDifferDefinitionsByHandlerId()
        {
            return _typeHandlers.ToDictionary(
                x => x.GetType().Name,
                x => GetCompatibleDiffersFor(x)
                    .OrderByDescending(x => x.UIOrder)
                    .Select(d => new HCComparisonDifferDefinition
                    {
                        Id = d.GetType().Name,
                        Name = d.Name,
                        Description = d.Description
                    }).ToList()
            );
        }

        /// <inheritdoc />
        public async Task<List<HCComparisonInstanceSelection>> GetFilteredOptionsAsync(string handlerId, HCComparisonTypeFilter filter)
        {
            var handler = GetHandlerById(handlerId);
            if (handler == null) return new List<HCComparisonInstanceSelection>();
            var results = await handler.GetFilteredOptionsAsync(filter);
            return results.ToList();
        }

        /// <inheritdoc />
        public async Task<HCComparisonMultiDifferOutput> ExecuteDiffAsync(string handlerId, string[] differIds, string leftId, string rightId)
        {
            var handler = GetHandlerById(handlerId);
            if (handler == null) return new HCComparisonMultiDifferOutput();
            var differs = GetCompatibleDiffersFor(handler)
                .Where(x => differIds.Contains(x.GetType().Name));
            if (!differs.Any()) return new HCComparisonMultiDifferOutput();

            var left = await handler.GetInstanceWithIdAsync(leftId);
            var right = await handler.GetInstanceWithIdAsync(rightId);
            var leftName = left == null ? "<null>" : handler.GetInstanceDisplayName(left);
            var rightName = right == null ? "<null>" : handler.GetInstanceDisplayName(right);

            var result = new HCComparisonMultiDifferOutput();
            foreach (var differ in differs)
            {
                var diffResult = await differ.CompareInstancesAsync(left, right, leftName, rightName);
                result.Data.Add(new HCComparisonMultiDifferSingleOutput
                {
                    DifferId = differ.GetType().Name,
                    Data = diffResult.Data
                });
            }
            return result;
        }

        private IHCComparisonTypeHandler GetHandlerById(string id)
            => _typeHandlers.FirstOrDefault(x => x.GetType().Name == id);

        private List<IHCComparisonDiffer> GetCompatibleDiffersFor(IHCComparisonTypeHandler handler)
            => _differs
                .Where(x => x.CanHandle(handler))
                .OrderByDescending(x => x.UIOrder)
                .ToList();

    }
}
