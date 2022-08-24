using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System;
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
        /// Get content to display for the type <paramref name="type"/> decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public async Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(Type type, object permutation)
        {
            if (permutation is TPermutation permutationTyped)
            {
                return await GetContentForAsync(permutationTyped);
            }
            return null;
        }

        /// <summary>
        /// Get content to display for the type <typeparamref name="TPermutation"/> decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public abstract Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(TPermutation permutation);
    }
}
