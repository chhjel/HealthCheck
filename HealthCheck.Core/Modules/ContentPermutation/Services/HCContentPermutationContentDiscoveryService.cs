using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ContentPermutation.Services
{
    /// <summary>
    /// Handles content discovery based on selected permutations.
    /// </summary>
    public class HCContentPermutationContentDiscoveryService : IHCContentPermutationContentDiscoveryService
    {
        private readonly IEnumerable<IHCContentPermutationContentHandler> _handlers;

        /// <summary>
        /// Handles content discovery based on selected permutations.
        /// </summary>
        public HCContentPermutationContentDiscoveryService(IEnumerable<IHCContentPermutationContentHandler> handlers)
        {
            _handlers = handlers;
        }

        /// <inheritdoc />
        public async Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(Type type, object permutation)
        {
            var content = new List<HCPermutatedContentItemViewModel>();
            foreach (var handler in _handlers)
            {
                var items = await handler.GetContentForAsync(type, permutation);
                if (items?.Any() == true) content.AddRange(items);
            }
            return content;
        }
    }
}
