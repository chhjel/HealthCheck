using EPiServer.Framework.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace HealthCheck.Episerver.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache.
    /// </summary>
    public abstract class HCEpiserverSingleBlobStorageBase<TData>
        where TData: new()
    {
        /// <summary>
        /// Defaults to the default provider if null.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Defaults to a hardcoded guid if null
        /// </summary>
        public Guid? ContainerId { get; set; }

        /// <summary>
        /// How long to cache data in memory for before fetching again from blob storage.
        /// <para>Defaults to 6 hours.</para>
        /// </summary>
        public virtual TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(6);

        /// <summary>
        /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
        /// </summary>
        protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

        /// <summary>
        /// Default id if <see cref="ContainerId"/> is not provided.
        /// </summary>
        protected abstract Guid DefaultContainerId { get; }

        /// <summary>
        /// Caches blob data.
        /// </summary>
        protected IMemoryCache Cache { get; }

        /// <summary>
        /// Key used to cache data in <see cref="Cache"/>
        /// </summary>
        protected virtual string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly IBlobFactory _blobFactory;

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCEpiserverSingleBlobStorageBase(IBlobFactory blobFactory, IMemoryCache cache)
        {
            _blobFactory = blobFactory;
            Cache = cache;
        }

        /// <summary>
        /// Get stored or cached blob data as <c>TData</c>.
        /// </summary>
        protected TData GetBlobData()
        {
            if (Cache != null && Cache.TryGetValue(CacheKey, out TData cachedData))
            {
                return cachedData;
            }

            var blob = CreateBlob();
            try
            {
                var bytes = blob.ReadAllBytes();
                if (bytes?.Any() != true)
                {
                    return new TData();
                }

                var json = Encoding.UTF8.GetString(bytes);
                var data = JsonConvert.DeserializeObject<TData>(json);
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

            var blob = CreateBlob();
            var json = JsonConvert.SerializeObject(data);

            using var stream = blob.OpenWrite();
            var writer = new StreamWriter(stream);
            writer.WriteLine(json);
            writer.Flush();
        }

        /// <summary>
        /// Invalidates any cached data.
        /// </summary>
        protected void InvalidateCache() => Cache?.Remove(CacheKey);

        private Uri GetContainerUri()
        {
            var containerId = ContainerIdWithFallback;
            if (ProviderName != null)
            {
                return Blob.GetContainerIdentifier(ProviderName, containerId);
            }
            return Blob.GetContainerIdentifier(containerId);
        }

        private Blob CreateBlob()
        {
            var container = GetContainerUri();
            return _blobFactory.CreateBlob(container, ".json");
        }
    }
}
