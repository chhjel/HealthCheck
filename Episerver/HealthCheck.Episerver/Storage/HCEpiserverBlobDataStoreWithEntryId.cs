using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Episerver.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores messages in blob storage.
    /// </summary>
    public class HCEpiserverBufferedBlobDataStoreWithEntryId<TItem, TId>
        : HCEpiserverSingleBufferedDictionaryBlobStorageBase<HCEpiserverBufferedBlobDataStoreWithEntryId<TItem, TId>.HCBlobDataStoreWithEntryIdData, TItem, TId>, IDataStoreWithEntryId<TItem>
    {
        /// <inheritdoc />
        protected override Guid DefaultContainerId => Guid.Parse("854e5461-314a-4718-8d09-a441597c50f4");

        private Func<TItem, TId> IdSelector { get; set; }

        /// <summary>
        /// Stores messages in blob storage.
        /// </summary>
        public HCEpiserverBufferedBlobDataStoreWithEntryId(IBlobFactory blobFactory, IMemoryCache cache, Func<TItem, TId> idSelector)
            : base(blobFactory, cache)
        {
            IdSelector = idSelector;
        }

        /// <summary>
        /// Store the given item. Replaces item if it already exists.
        /// The <paramref name="update"/> parameter is not used.
        /// </summary>
        public TItem InsertOrUpdateItem(TItem item, Func<TItem, TItem> update = null)
        {
            InsertItemBuffered(item);
            return item;
        }

        /// <inheritdoc />
        public void InsertOrUpdateItems(IEnumerable<TItem> items)
            => InsertItemsBuffered(items);

        /// <inheritdoc />
        public IEnumerable<TItem> GetEnumerable()
        {
            var data = GetBlobData();
            return data?.Items.Values ?? Enumerable.Empty<TItem>();
        }

        /// <inheritdoc />
        protected override TId GetItemId(TItem item)
            => IdSelector(item);

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
