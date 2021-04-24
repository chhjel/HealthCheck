using Microsoft.Extensions.Caching.Memory;
using System;

namespace HealthCheck.Utility.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache.
    /// </summary>
    public abstract class HCSingleBlobStorageBase<TData>
        where TData: new()
    {
        /// <summary>
        /// How long to cache data in memory for before fetching again from blob storage.
        /// <para>Defaults to 6 hours.</para>
        /// </summary>
        public virtual TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(6);

        /// <summary>
        /// Caches blob data.
        /// </summary>
        protected IMemoryCache Cache { get; }

        /// <summary>
        /// Key used to cache data in <see cref="Cache"/>
        /// </summary>
        protected abstract string CacheKey { get; }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCSingleBlobStorageBase(IMemoryCache cache)
        {
            Cache = cache;
        }

        /// <summary>
        /// Get the blob data.
        /// </summary>
        protected abstract TData RetrieveBlobData();

        /// <summary>
        /// Store the blob data.
        /// </summary>
        protected abstract void StoreBlobData(TData data);

        /// <summary>
        /// Get stored or cached blob data as <c>TData</c>.
        /// </summary>
        protected TData GetBlobData()
        {
            if (Cache != null && Cache.TryGetValue(CacheKey, out TData cachedData))
            {
                return cachedData;
            }

            try
            {
                var data = RetrieveBlobData();
                if (data == null)
                {
                    return new TData();
                }

                Cache?.Set(CacheKey, data, CacheDuration);
                return data;
            }
            catch(Exception)
            {
                return new TData();
            }
        }

        /// <summary>
        /// Save blob data and update cache.
        /// </summary>
        protected void SaveBlobData(TData data)
        {
            Cache?.Set(CacheKey, data, CacheDuration);
            StoreBlobData(data);
        }

        /// <summary>
        /// Invalidates any cached data.
        /// </summary>
        protected void InvalidateCache() => Cache?.Remove(CacheKey);
    }
}
