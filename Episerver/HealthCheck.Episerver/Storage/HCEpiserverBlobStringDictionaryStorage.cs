using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Utility.Storage.Abstractions;
using HealthCheck.Episerver.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores a dictionary in blob storage.
    /// </summary>
    public class HCEpiserverBlobStringDictionaryStorage
        : HCSingleBlobStorageBase<HCEpiserverBlobStringDictionaryStorage.HCDictionaryBlobData>, IHCStringDictionaryStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("5dd58b96-06f2-4629-a8d4-82e01680bf79");

        /// <summary>
        /// Defaults to the default provider if null.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Defaults to a hardcoded guid if null
        /// </summary>
        public Guid? ContainerId { get; set; }

        /// <summary>
        /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
        /// </summary>
        protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

        /// <inheritdoc />
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly EpiserverBlobHelper<HCDictionaryBlobData> _blobHelper;

        /// <summary>
        /// Stores a dictionary in blob storage.
        /// </summary>
        public HCEpiserverBlobStringDictionaryStorage(IBlobFactory blobFactory, IMemoryCache cache) : base(cache)
        {
            _blobHelper = new EpiserverBlobHelper<HCDictionaryBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetValues()
        {
            var data = GetBlobData();
            return data?.Dictionary ?? new Dictionary<string, string>();
        }

        /// <inheritdoc />
        public void SaveValues(Dictionary<string, string> values)
        {
            SaveBlobData(new HCDictionaryBlobData
            {
                Dictionary = values ?? new Dictionary<string, string>()
            });
        }

        /// <inheritdoc />
        protected override HCDictionaryBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCDictionaryBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCDictionaryBlobData
        {
            /// <summary>
            /// Stored values.
            /// </summary>
            public Dictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>();
        }
    }
}
