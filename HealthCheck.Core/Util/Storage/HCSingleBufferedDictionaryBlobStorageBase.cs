using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util.Storage
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId>
        : HCSingleBlobStorageBase<TData>
        where TData : HCSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId>.IBufferedBlobDictionaryStorageData, new()
    {
        /// <summary>
        /// Max duration to buffer data in memory for before writing to blob storage.
        /// <para>Defaults to 10 seconds.</para>
        /// </summary>
        public virtual TimeSpan BlobUpdateBufferDuration { get => BufferQueue.Delay; set => BufferQueue.Delay = value; }

        /// <summary>
        /// Max buffer size.
        /// <para>Defaults to 100, and is usually set to a suitable number per implementation.</para>
        /// </summary>
        public virtual int MaxBufferSize { get => BufferQueue.QueueSizeLimit; set => BufferQueue.QueueSizeLimit = value; }

        /// <summary>
        /// The buffered queue of items to add.
        /// </summary>
        protected readonly DelayedBufferDictionary<TId, BufferQueueItem> BufferQueue;

        /// <summary>
        /// Optionally limit the max number of items to store.
        /// <para>The first n items will be kept.</para>
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
        /// Wrapper type for buffered items.
        /// </summary>
        protected struct BufferQueueItem
        {
            /// <summary></summary>
            public TId Id { get; set; }

            /// <summary></summary>
            public object GroupId { get; set; }

            /// <summary>
            /// Buffered item.
            /// </summary>
            public TItem Item { get; set; }

            /// <summary>
            /// True if it's an update.
            /// </summary>
            public bool IsUpdate { get; set; }

            /// <summary>
            /// Summary.
            /// </summary>
            public override string ToString() => $"[{Id}] {(Item?.ToString() ?? "null")}";
        }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCSingleBufferedDictionaryBlobStorageBase(IHCCache cache)
            : base(cache)
        {
            BufferQueue = new DelayedBufferDictionary<TId, BufferQueueItem>(x => x.Id, OnBufferCallback, TimeSpan.FromSeconds(10), 100);
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

        /// <summary>
        /// Sets <see cref="MaxItemAge"/>, the max age of items before they will be deleted on the next update/insert.
        /// </summary>
        public HCSingleBufferedDictionaryBlobStorageBase<TData, TItem, TId> SetMaxItemAge(TimeSpan? maxAge)
        {
            MaxItemAge = maxAge;
            return this;
        }

        /// <summary>
        /// Get the timestamp of an item.
        /// </summary>
        protected virtual DateTimeOffset GetItemTimestamp(TItem item) => default;

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected virtual void InsertItemBuffered(TItem item, TId id, object groupId = null, bool isUpdate = false)
        {
            var data = new BufferQueueItem { Item = item, Id = id, GroupId = groupId, IsUpdate = isUpdate };
            if (isUpdate && BufferQueue.TryGet(id, out var existing) && !existing.IsUpdate)
            {
                data.IsUpdate = false;
            }
            BufferQueue.Set(data);
        }

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemsBuffered(IEnumerable<TItem> items, Func<TItem, TId> idSelector)
            => BufferQueue.Set(items.Select(x => new BufferQueueItem { Item = x, Id = idSelector(x) }));

        /// <summary>
        /// Removes an item from both buffer and storage. Instantly saves if data was stored.
        /// </summary>
        public void RemoveItem(TId id)
        {
            RemoveFromBufferQueue(id);

            var data = GetBlobData();
            if (data.Items.ContainsKey(id))
            {
                data.Items.Remove(id);
                SaveBlobData(data);
            }
        }

        /// <summary>
        /// Removes matching item from the buffer queue.
        /// </summary>
        protected void RemoveFromBufferQueue(TId id)
            => BufferQueue.Remove(id);

        /// <summary>
        /// Get all buffered new items not yet stored.
        /// </summary>
        protected IEnumerable<TItem> GetBufferedItems()
            => BufferQueue.GetBufferedItems().Select(x => x.Item);

        /// <summary>
        /// Called when the buffer is full or duration has been reached.
        /// </summary>
        protected virtual void OnBufferCallback(Dictionary<TId, BufferQueueItem> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            var data = GetBlobData();
            data = UpdateDataFromBuffer(data, items);
            SaveBlobData(data);
        }

        /// <summary>
        /// Update the actual data here before its saved.
        /// </summary>
        protected virtual TData UpdateDataFromBuffer(TData data, Dictionary<TId, BufferQueueItem> bufferedItems)
        {
            foreach (var item in bufferedItems)
            {
                var id = item.Key;
                if (item.Value.IsUpdate && !data.Items.ContainsKey(id))
                {
                    continue;
                }
                data.Items[id] = item.Value.Item;
            }

            if (MaxItemCount != null && data.Items.Count > MaxItemCount)
            {
                var skipCount = data.Items.Count - MaxItemCount.Value;
                data.Items = data.Items.Skip(skipCount).ToDictionary(x => x.Key, x => x.Value);
            }

            if (SupportsMaxItemAge && MaxItemAge != null)
            {
                var toRemove = data.Items.Where(x => (DateTimeOffset.Now - GetItemTimestamp(x.Value)) > MaxItemAge).ToArray();
                foreach(var item in toRemove)
                {
                    data.Items.Remove(item.Key);
                }
            }

            return data;
        }

        /// <summary>
        /// Get all items, buffered or stored.
        /// </summary>
        protected IEnumerable<TItem> GetItems()
        {
            var checkedBufferedIds = new HashSet<TId>();
            foreach (var item in BufferQueue.GetBufferedItems())
            {
                checkedBufferedIds.Add(item.Id);
                yield return item.Item;
            }

            var data = GetBlobData();
            if (data != null)
            {
                foreach (var item in data.Items)
                {
                    if (!checkedBufferedIds.Contains(item.Key))
                    {
                        yield return item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Skips waiting for the buffer and processes it at once.
        /// <para>Should not be needed for other things than unit testing.</para>
        /// </summary>
        public void ForceBufferCallback() => BufferQueue.ForceBufferCallback();

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
