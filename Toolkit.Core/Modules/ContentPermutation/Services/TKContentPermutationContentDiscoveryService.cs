using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Services
{
    /// <summary>
    /// Handles content discovery based on selected permutations.
    /// </summary>
    public class TKContentPermutationContentDiscoveryService : ITKContentPermutationContentDiscoveryService
    {
        private readonly IEnumerable<ITKContentPermutationContentHandler> _handlers;

        /// <summary>
        /// Handles content discovery based on selected permutations.
        /// </summary>
        public TKContentPermutationContentDiscoveryService(IEnumerable<ITKContentPermutationContentHandler> handlers)
        {
            _handlers = handlers;
        }

        /// <inheritdoc />
        public async Task<TKPermutatedContentResultViewModel> GetContentForAsync(TKContentPermutationType type, TKGetContentPermutationContentOptions options)
        {
            var content = new List<TKPermutatedContentItemViewModel>();
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
            return new TKPermutatedContentResultViewModel
            {
                Content = content,
                WasCached = wasCached
            };
        }
    }
}
