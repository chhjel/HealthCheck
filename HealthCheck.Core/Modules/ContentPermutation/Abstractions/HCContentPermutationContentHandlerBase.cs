using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ContentPermutation.Abstractions
{
    /// <summary>
    /// Handles discovering content to display for filtered types.
    /// <para>Supports caching if <see cref="CacheDuration"/> is set.</para>
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
        public async Task<HCPermutatedContentResultViewModel> GetContentForAsync(HCContentPermutationType type, HCGetContentPermutationContentOptions options)
        {
            string cacheKey = null;
            if (IsCacheEnabled)
            {
                var cacheKeySuffix = HCGlobalConfig.Serializer.Serialize(options.PermutationObj, pretty: false);
                cacheKey = $"{options.MaxCount}|{cacheKeySuffix}";
                if (_cache.TryGetValue<List<HCPermutatedContentItemViewModel>>(cacheKey, out var cachedValue))
                {
                    return new HCPermutatedContentResultViewModel
                    {
                        Content = cachedValue,
                        WasCached = true
                    };
                }

                // Check caches w/ other counts
                if (type.MaxAllowedContentCount <= 1000)
                {
                    for (int i = type.MaxAllowedContentCount; i > 0; i--)
                    {
                        var relatedCacheKey = $"{i}|{cacheKeySuffix}";
                        if (_cache.TryGetValue<List<HCPermutatedContentItemViewModel>>(relatedCacheKey, out var relatedCachedValue))
                        {
                            return new HCPermutatedContentResultViewModel
                            {
                                Content = relatedCachedValue.Take(options.MaxCount).ToList(),
                                WasCached = true
                            };
                        }
                    }
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
                return new HCPermutatedContentResultViewModel
                {
                    Content = content,
                    WasCached = false
                };
            }

            return null;
        }

        /// <summary>
        /// Get content to display for the type <typeparamref name="TPermutation"/> decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public abstract Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions<TPermutation> options);
    }
}
