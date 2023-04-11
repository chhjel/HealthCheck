using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Core.Util.Storage;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Episerver.Storage
{
    /// <summary>
    /// Stores a dictionary in blob storage.
    /// </summary>
    public class TKEpiserverBlobStringDictionaryStorage
        : TKSingleBlobStorageBase<TKEpiserverBlobStringDictionaryStorage.TKDictionaryBlobData>, ITKStringDictionaryStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("89d58b96-06f2-4629-a8d4-82e01680bf79");

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
        protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

        private readonly TKEpiserverBlobHelper<TKDictionaryBlobData> _blobHelper;

        /// <summary>
        /// Stores a dictionary in blob storage.
        /// </summary>
        public TKEpiserverBlobStringDictionaryStorage(IBlobFactory blobFactory, ITKCache cache) : base(cache)
        {
            _blobHelper = new TKEpiserverBlobHelper<TKDictionaryBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
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
            SaveBlobData(new TKDictionaryBlobData
            {
                Dictionary = values ?? new Dictionary<string, string>()
            });
        }

        /// <inheritdoc />
        protected override TKDictionaryBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(TKDictionaryBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class TKDictionaryBlobData
        {
            /// <summary>
            /// Stored values.
            /// </summary>
            public Dictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>();
        }
    }
}
