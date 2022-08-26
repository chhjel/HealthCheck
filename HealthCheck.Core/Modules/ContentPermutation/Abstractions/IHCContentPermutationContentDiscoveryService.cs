using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ContentPermutation.Abstractions
{
    /// <summary>
    /// Handles content discovery based on selected permutations.
    /// </summary>
    public interface IHCContentPermutationContentDiscoveryService
    {
        /// <summary>
        /// Get content to display for a type decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        Task<HCPermutatedContentResultViewModel> GetContentForAsync(HCContentPermutationType type, HCGetContentPermutationContentOptions options);
    }
}
