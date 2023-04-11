using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Comparison.Abstractions
{
    /// <summary>
    /// Handles content discovery based on selected permutations.
    /// </summary>
    public interface ITKComparisonService
    {
        /// <summary>
        /// Get available types.
        /// </summary>
        List<TKComparisonTypeDefinition> GetComparisonTypeDefinitions();

        /// <summary>
        /// Get available differs.
        /// </summary>
        Dictionary<string, List<TKComparisonDifferDefinition>> GetDifferDefinitionsByHandlerId();

        /// <summary>
        /// Get content choices to select in the UI.
        /// </summary>
        Task<List<TKComparisonInstanceSelection>> GetFilteredOptionsAsync(string handlerId, TKComparisonTypeFilter filter);

        /// <summary>
        /// Execute a comparison between the given left and right id using the given handler.
        /// </summary>
        Task<TKComparisonMultiDifferOutput> ExecuteDiffAsync(string handlerId, string[] differIds, string leftId, string rightId);
    }
}