using HealthCheck.Core.Modules.Comparison.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles content discovery based on selected permutations.
    /// </summary>
    public interface IHCComparisonService
    {
        /// <summary>
        /// Get available types.
        /// </summary>
        List<HCComparisonTypeDefinition> GetComparisonTypeDefinitions();

        /// <summary>
        /// Get available differs.
        /// </summary>
        Dictionary<string, List<HCComparisonDifferDefinition>> GetDifferDefinitionsByHandlerId();

        /// <summary>
        /// Get content choices to select in the UI.
        /// </summary>
        Task<List<HCComparisonInstanceSelection>> GetFilteredOptionsAsync(string handlerId, HCComparisonTypeFilter filter);

        /// <summary>
        /// Execute a comparison between the given left and right id using the given handler.
        /// </summary>
        Task<HCComparisonMultiDifferOutput> ExecuteDiffAsync(string handlerId, string[] differIds, string leftId, string rightId);
    }
}