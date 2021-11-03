using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Episerver.Utils;
using HealthCheck.Core.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores messages in blob storage.
    /// </summary>
    public class HCEpiserverBufferedBlobDataStoreWithEntryId<TItem, TId>
        : HCSingleBufferedDictionaryBlobStorageBase<HCEpiserverBufferedBlobDataStoreWithEntryId<TItem, TId>.HCBlobDataStoreWithEntryIdData, TItem, TId>, IDataStoreWithEntryId<TItem>
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("874e5461-314a-4718-8d09-a441597c50f4");

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

        private readonly HCEpiserverBlobHelper<HCBlobDataStoreWithEntryIdData> _blobHelper;

        private Func<TItem, TId> IdSelector { get; set; }

        /// <summary>
        /// Stores messages in blob storage.
        /// </summary>
        public HCEpiserverBufferedBlobDataStoreWithEntryId(IBlobFactory blobFactory, IHCCache cache, Func<TItem, TId> idSelector)
            : base(cache)
        {
            IdSelector = idSelector;
            _blobHelper = new HCEpiserverBlobHelper<HCBlobDataStoreWithEntryIdData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <summary>
        /// Store the given item. Replaces item if it already exists.
        /// The <paramref name="update"/> parameter is not used.
        /// </summary>
        public TItem InsertOrUpdateItem(TItem item, Func<TItem, TItem> update = null)
        {
            InsertItemBuffered(item, IdSelector(item));
            return item;
        }

        /// <inheritdoc />
        public void InsertOrUpdateItems(IEnumerable<TItem> items)
            => InsertItemsBuffered(items, IdSelector);

        /// <inheritdoc />
        public IEnumerable<TItem> GetEnumerable()
        {
            foreach (var item in GetBufferedItems())
            {
                yield return item;
            }

            var data = GetBlobData();
            if (data?.Items != null)
            {
                foreach (var item in data.Items.Values ?? Enumerable.Empty<TItem>())
                {
                    yield return item;
                }
            }
        }

        /// <inheritdoc />
        protected override HCBlobDataStoreWithEntryIdData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCBlobDataStoreWithEntryIdData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCBlobDataStoreWithEntryIdData : IBufferedBlobDictionaryStorageData
        {
            /// <inheritdoc />
            public Dictionary<TId, TItem> Items { get; set; } = new Dictionary<TId, TItem>();
        }
    }
}
