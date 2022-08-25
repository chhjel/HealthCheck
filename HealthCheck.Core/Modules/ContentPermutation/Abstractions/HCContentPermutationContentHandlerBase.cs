using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using HealthCheck.Core.Util;
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
        /// Optional cache duration for the resolved content.
        /// </summary>
        protected virtual TimeSpan? CacheDuration { get; }

        private static readonly HCSimpleMemoryCache<List<HCPermutatedContentItemViewModel>> _cache = new();
        private bool IsCacheEnabled => CacheDuration != null && HCGlobalConfig.Serializer != null;

        /// <summary>
        /// Get content to display for a type decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public async Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions options)
        {
            string cacheKey = null;
            if (IsCacheEnabled)
            {
                cacheKey = HCGlobalConfig.Serializer.Serialize(options.PermutationObj, pretty: false);
                if (_cache.TryGetValue<List<HCPermutatedContentItemViewModel>>(cacheKey, out var cachedValue))
                {
                    return cachedValue;
                }
            }

            if (options.PermutationObj is TPermutation permutationTyped)
            {
                var opts = new HCGetContentPermutationContentOptions<TPermutation>
                {
                    PermutationObj = options.PermutationObj,
                    Permutation = permutationTyped,
                    Type = options.Type,
                    MaxCount = options.MaxCount
                };

                var content = await GetContentForAsync(opts);
                if (IsCacheEnabled)
                {
                    _cache.Set(cacheKey, content, CacheDuration.Value);
                }
                return content;
            }
            return null;
        }

        /// <summary>
        /// Get content to display for the type <typeparamref name="TPermutation"/> decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public abstract Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions<TPermutation> options);
    }
}
