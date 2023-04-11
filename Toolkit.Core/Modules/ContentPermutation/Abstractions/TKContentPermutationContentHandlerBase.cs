using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;

/// <summary>
/// Handles discovering content to display for filtered types.
/// <para>Supports caching if <see cref="CacheDuration"/> is set.</para>
/// </summary>
public abstract class TKContentPermutationContentHandlerBase<TPermutation> : ITKContentPermutationContentHandler
    where TPermutation : class, new()
{
    /// <summary>
    /// Optional cache duration for the resolved content.
    /// </summary>
    protected virtual TimeSpan? CacheDuration { get; }

    private static readonly TKSimpleMemoryCache<List<TKPermutatedContentItemViewModel>> _cache = new();
    private bool IsCacheEnabled => CacheDuration != null && TKGlobalConfig.Serializer != null;

    /// <summary>
    /// Get content to display for a type decorated with <see cref="TKContentPermutationTypeAttribute"/>.
    /// </summary>
    public async Task<TKPermutatedContentResultViewModel> GetContentForAsync(TKContentPermutationType type, TKGetContentPermutationContentOptions options)
    {
        string cacheKey = null;
        if (IsCacheEnabled)
        {
            var cacheKeySuffix = TKGlobalConfig.Serializer.Serialize(options.PermutationObj, pretty: false);
            cacheKey = $"{options.MaxCount}|{cacheKeySuffix}";
            if (_cache.TryGetValue<List<TKPermutatedContentItemViewModel>>(cacheKey, out var cachedValue))
            {
                return new TKPermutatedContentResultViewModel
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
                    if (_cache.TryGetValue<List<TKPermutatedContentItemViewModel>>(relatedCacheKey, out var relatedCachedValue))
                    {
                        return new TKPermutatedContentResultViewModel
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
            var opts = new TKGetContentPermutationContentOptions<TPermutation>
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
            return new TKPermutatedContentResultViewModel
            {
                Content = content,
                WasCached = false
            };
        }

        return null;
    }

    /// <summary>
    /// Get content to display for the type <typeparamref name="TPermutation"/> decorated with <see cref="TKContentPermutationTypeAttribute"/>.
    /// </summary>
    public abstract Task<List<TKPermutatedContentItemViewModel>> GetContentForAsync(TKGetContentPermutationContentOptions<TPermutation> options);
}
