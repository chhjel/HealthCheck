using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ContentPermutation.Abstractions
{

    /// <summary>
    /// Handles discovering content to display for filtered types.
    /// </summary>
    public interface IHCContentPermutationContentHandler
    {
        /// <summary>
        /// Get content to display for a type decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions options);
    }
}
