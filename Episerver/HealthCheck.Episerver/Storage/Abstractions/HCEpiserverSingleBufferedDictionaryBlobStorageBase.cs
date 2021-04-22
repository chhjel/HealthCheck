using EPiServer.Framework.Blobs;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCEpiserverSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId>
        : HCEpiserverSingleBufferedBlobStorageBase<TData, TItem>
        where TData : HCEpiserverSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId>.IBufferedBlobDictionaryStorageData, new()
    {
        /// <summary>
        /// Optionally limit the max number of items to store.
        /// <para>The first n items will be kept.</para>
        /// </summary>
        public virtual int? MaxItemCount { get; set; }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCEpiserverSingleBufferedDictionaryBlobStorageBase(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
        }

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<TItem> bufferedItems)
        {
            foreach (var item in bufferedItems)
            {
                var id = GetItemId(item);
                data.Items[id] = item;
            }

            if (MaxItemCount != null && data.Items.Count > MaxItemCount)
            {
                var skipCount = data.Items.Count - MaxItemCount.Value;
                data.Items = data.Items.Skip(skipCount).ToDictionary(x => x.Key, x => x.Value);
            }

            return data;
        }

        /// <summary>
        /// Get the id of the given item.
        /// </summary>
        protected abstract TId GetItemId(TItem item);

        /// <summary>
        /// Stored data model.
        /// </summary>
        public interface IBufferedBlobDictionaryStorageData
        {
            /// <summary>
            /// Stored items.
            /// </summary>
            Dictionary<TId, TItem> Items { get; set; }
        }
    }
}
