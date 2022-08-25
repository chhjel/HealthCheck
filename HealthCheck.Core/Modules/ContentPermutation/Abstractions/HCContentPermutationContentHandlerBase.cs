using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ContentPermutation.Abstractions
{
    /// <summary>
    /// Handles discovering content to display for filtered types.
    /// </summary>
    public abstract class HCContentPermutationContentHandlerBase<TPermutation> : IHCContentPermutationContentHandler
        where TPermutation : class, new()
    {
        /// <summary>
        /// Get content to display for a type decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public async Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions options)
        {
            if (options.PermutationObj is TPermutation permutationTyped)
            {
                var opts = new HCGetContentPermutationContentOptions<TPermutation>
                {
                    PermutationObj = options.PermutationObj,
                    Permutation = permutationTyped,
                    Type = options.Type,
                    MaxCount = options.MaxCount
                };
                return await GetContentForAsync(opts);
            }
            return null;
        }

        /// <summary>
        /// Get content to display for the type <typeparamref name="TPermutation"/> decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public abstract Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions<TPermutation> options);
    }
}
