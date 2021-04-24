using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Utility.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId>
        : HCSingleBufferedBlobStorageBase<TData, TItem>
        where TData : HCSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId>.IBufferedBlobDictionaryStorageData, new()
    {
        /// <summary>
        /// Optionally limit the max number of items to store.
        /// <para>The first n items will be kept.</para>
        /// </summary>
        public virtual int? MaxItemCount { get; set; }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCSingleBufferedDictionaryBlobStorageBase(IMemoryCache cache)
            : base(cache)
        {
        }

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems)
        {
            foreach (var item in bufferedItems)
            {
                var id = GetItemId(item.Item);
                data.Items[id] = item.Item;
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
