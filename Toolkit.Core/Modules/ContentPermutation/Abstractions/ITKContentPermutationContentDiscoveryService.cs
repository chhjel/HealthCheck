using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;

/// <summary>
/// Handles content discovery based on selected permutations.
/// </summary>
public interface ITKContentPermutationContentDiscoveryService
{
    /// <summary>
    /// Get content to display for a type decorated with <see cref="TKContentPermutationTypeAttribute"/>.
    /// </summary>
    Task<TKPermutatedContentResultViewModel> GetContentForAsync(TKContentPermutationType type, TKGetContentPermutationContentOptions options);
}
