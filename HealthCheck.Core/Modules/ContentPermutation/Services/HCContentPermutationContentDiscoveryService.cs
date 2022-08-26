using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Models;
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
        public async Task<HCPermutatedContentResultViewModel> GetContentForAsync(HCContentPermutationType type, HCGetContentPermutationContentOptions options)
        {
            var content = new List<HCPermutatedContentItemViewModel>();
            var originalMaxCount = options.MaxCount;
            var wasCached = false;
            foreach (var handler in _handlers)
            {
                // Per handler max count
                options.MaxCount = originalMaxCount.Clamp(1, type.MaxAllowedContentCount);

                var contentResult = await handler.GetContentForAsync(type, options);
                if (contentResult?.Content?.Any() == true) content.AddRange(contentResult.Content);
                if (contentResult?.WasCached == true) wasCached = true;
            }
            return new HCPermutatedContentResultViewModel
            {
                Content = content,
                WasCached = wasCached
            };
        }
    }
}
