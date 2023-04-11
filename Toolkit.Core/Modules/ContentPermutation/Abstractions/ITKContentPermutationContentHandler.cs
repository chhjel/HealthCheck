using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions
{

    /// <summary>
    /// Handles discovering content to display for filtered types.
    /// </summary>
    public interface ITKContentPermutationContentHandler
    {
        /// <summary>
        /// Get content to display for a type decorated with <see cref="TKContentPermutationTypeAttribute"/>.
        /// </summary>
        Task<TKPermutatedContentResultViewModel> GetContentForAsync(TKContentPermutationType type, TKGetContentPermutationContentOptions options);
    }
}
