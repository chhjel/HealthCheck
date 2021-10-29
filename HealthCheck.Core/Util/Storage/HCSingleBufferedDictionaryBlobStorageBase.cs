using HealthCheck.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util.Storage
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
        protected HCSingleBufferedDictionaryBlobStorageBase(IHCCache cache)
            : base(cache)
        {
        }

        /// <summary>
        /// Sets <see cref="MaxItemCount"/>, the max number of items to store.
        /// <para>The first n items will be kept.</para>
        /// </summary>
        public HCSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId> SetMaxItemCount(int? maxCount)
        {
            MaxItemCount = maxCount;
            return this;
        }

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems)
        {
            foreach (var item in bufferedItems)
            {
                if (item.IsInsert)
                {
                    var id = GetItemId(item.ItemToInsert);
                    data.Items[id] = item.ItemToInsert;
                }
                else if (item.IsUpdate && item.Id is TId id)
                {
                    var existingItem = data.Items.FirstOrDefault(x => GetItemId(x.Value)?.ToString() == id?.ToString()).Value;
                    if (existingItem != null)
                    {
                        item.UpdateAction(existingItem);
                    }
                }
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
