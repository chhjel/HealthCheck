using HealthCheck.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util.Storage
{
    /// <summary>
    /// Base implementation for storing a list in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCSingleBufferedListBlobStorageBase<TData, TItem> : HCSingleBufferedBlobStorageBase<TData, TItem>
        where TData : HCSingleBufferedListBlobStorageBase<TData, TItem>.IBufferedBlobListStorageData, new()
    {
        /// <summary>
        /// Optionally limit the max number of latest items to store.
        /// </summary>
        public virtual int? MaxItemCount { get; set; }

        /// <summary>
        /// Optionally limit the max age of items to store.
        /// </summary>
        public virtual TimeSpan? MaxItemAge { get; set; }

        /// <summary>
        /// True if GetItemTimestamp is implemented.
        /// </summary>
        protected bool SupportsMaxItemAge { get; set; }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCSingleBufferedListBlobStorageBase(IHCCache cache)
            : base(cache)
        {
        }

        /// <summary>
        /// Sets <see cref="MaxItemCount"/>, the max number of items to store.
        /// <para>The first n items will be kept.</para>
        /// </summary>
        public HCSingleBufferedListBlobStorageBase<TData, TItem> SetMaxItemCount(int? maxCount)
        {
            MaxItemCount = maxCount;
            return this;
        }

        /// <summary>
        /// Sets <see cref="MaxItemAge"/>, the max age of items before they will be deleted on the next update/insert.
        /// </summary>
        public HCSingleBufferedListBlobStorageBase<TData, TItem> SetMaxItemAge(TimeSpan? maxAge)
        {
            MaxItemAge = maxAge;
            return this;
        }

        /// <summary>
        /// Get the timestamp of an item.
        /// </summary>
        protected virtual DateTimeOffset GetItemTimestamp(TItem item) => default;

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems)
        {
            var toInsert = bufferedItems.Where(x => x.IsInsert).Select(x => x.ItemToInsert);
            data.Items.AddRange(toInsert.Select(x => x));

            var toUpdate = bufferedItems.Where(x => x.IsUpdate);
            foreach (var action in toUpdate)
            {
                var existing = data.Items.FirstOrDefault(x => GetItemId(x) == action.Id);
                if (existing != null)
                {
                    action.UpdateAction(existing);
                }
            }

            if (MaxItemCount != null && data.Items.Count > MaxItemCount)
            {
                var skipCount = data.Items.Count - MaxItemCount.Value;
                data.Items.RemoveRange(0, skipCount);
            }

            if (SupportsMaxItemAge && MaxItemAge != null)
            {
                data.Items.RemoveAll(x => (DateTimeOffset.Now - GetItemTimestamp(x)) > MaxItemAge);
            }

            return data;
        }

        /// <summary>
        /// For buffered updates to work the id must be known.
        /// </summary>
        protected virtual object GetItemId(TItem item) => default;

        /// <summary>
        /// Get all items, buffered or stored.
        /// </summary>
        protected IEnumerable<TItem> GetItems()
        {
            foreach (var item in GetBufferedItemsToInsert())
            {
                yield return item;
            }

            var data = GetBlobData();
            if (data != null)
            {
                foreach (var item in data.Items)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Stored data model.
        /// </summary>
        public interface IBufferedBlobListStorageData
        {
            /// <summary>
            /// Stored items.
            /// </summary>
            List<TItem> Items { get; set; }
        }
    }
}
