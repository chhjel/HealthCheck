using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Episerver.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores a dictionary in blob storage.
    /// </summary>
    public class HCEpiserverBlobStringDictionaryStorage
        : HCEpiserverSingleBlobStorageBase<HCEpiserverBlobStringDictionaryStorage.HCDictionaryBlobData>, IHCStringDictionaryStorage
    {
        /// <inheritdoc />
        protected override Guid DefaultContainerId => Guid.Parse("5dd58b96-06f2-4629-a8d4-82e01680bf79");

        /// <summary>
        /// Stores a dictionary in blob storage.
        /// </summary>
        public HCEpiserverBlobStringDictionaryStorage(IBlobFactory blobFactory, IMemoryCache cache) : base(blobFactory, cache)
        {
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
