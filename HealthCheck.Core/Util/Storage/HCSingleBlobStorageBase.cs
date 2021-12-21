﻿using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Metrics.Context;
using System;

namespace HealthCheck.Core.Util.Storage
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
        protected IHCCache Cache { get; }

        /// <summary>
        /// Key used to cache data in <see cref="Cache"/>
        /// </summary>
        protected abstract string CacheKey { get; }

        /// <summary>
        /// Number of time blob was saved.
        /// </summary>
        public long SaveCounter { get; private set; }

        /// <summary>
        /// Number of time blob was loaded.
        /// </summary>
        public long LoadCounter { get; private set; }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCSingleBlobStorageBase(IHCCache cache)
        {
            Cache = cache;
        }

        /// <summary>
        /// Invokes the given action with this object as the parameter.
        /// <para>For use in method chained configuration.</para>
        /// </summary>
        public HCSingleBlobStorageBase<TData> Configure(Action<HCSingleBlobStorageBase<TData>> action)
        {
            action?.Invoke(this);
            return this;
        }

        /// <summary>
        /// How long to cache data in memory for before fetching again from blob storage.
        /// </summary>
        public HCSingleBlobStorageBase<TData> SetCacheDuration(TimeSpan duration)
        {
            CacheDuration = duration;
            return this;
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
        public TData GetBlobData()
        {
            if (Cache != null && Cache.TryGetValue(CacheKey, out TData cachedData))
            {
                return cachedData;
            }

            try
            {
                var data = RetrieveBlobData() ?? new TData();

                Cache?.Set(CacheKey, data, CacheDuration);
                HCMetricsContext.IncrementGlobalCounter($"{GetType().GetFriendlyTypeName()}.RetrieveBlobData()");
                if (LoadCounter < long.MaxValue) LoadCounter++;
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
        protected virtual void SaveBlobData(TData data)
        {
            Cache?.Set(CacheKey, data, CacheDuration);
            StoreBlobData(data);
            HCMetricsContext.IncrementGlobalCounter($"{GetType().GetFriendlyTypeName()}.SaveBlobData()");
            if (SaveCounter < long.MaxValue) SaveCounter++;
        }

        /// <summary>
        /// Invalidates any cached data.
        /// </summary>
        protected void InvalidateCache() => Cache?.Remove(CacheKey);
    }
}
