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
    public abstract class HCSingleBufferedBlobStorageBase<TData, TItem> : HCSingleBlobStorageBase<TData>
        where TData : new()
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
        protected readonly DelayedBufferQueue<BufferQueueItem> BufferQueue;

        /// <summary>
        /// Wrapper type for buffered items.
        /// </summary>
        protected struct BufferQueueItem
        {
            /// <summary></summary>
            public object Id { get; set; }

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
            public override string ToString() => $"{(Item?.ToString() ?? "null")}";
        }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCSingleBufferedBlobStorageBase(IHCCache cache)
            : base(cache)
        {
            BufferQueue = new DelayedBufferQueue<BufferQueueItem>(OnBufferCallback, TimeSpan.FromSeconds(10), 100);
        }

        /// <summary>
        /// Max buffer size.
        /// </summary>
        public HCSingleBufferedBlobStorageBase<TData, TItem> SetMaxBufferSize(int maxCount)
        {
            MaxBufferSize = maxCount;
            return this;
        }

        /// <summary>
        /// Max duration to buffer data in memory for before writing to blob storage.
        /// </summary>
        public HCSingleBufferedBlobStorageBase<TData, TItem> SetBlobUpdateBufferDuration(TimeSpan duration)
        {
            BlobUpdateBufferDuration = duration;
            return this;
        }

        /// <summary>
        /// For buffered updates to work the id must be known.
        /// </summary>
        protected virtual object GetItemId(TItem item) => default;

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected virtual void InsertItemBuffered(TItem item, object id = null, object groupId = null, bool isUpdate = false, Action<BufferQueueItem> config = null)
        {
            var data = new BufferQueueItem { Item = item, Id = id, GroupId = groupId, IsUpdate = isUpdate };
            config?.Invoke(data);
            BufferQueue.Add(data);
        }

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemsBuffered(IEnumerable<TItem> items)
            => BufferQueue.Add(items.Select(x => new BufferQueueItem { Item = x }));

        /// <summary>
        /// Queues up an update to an item and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// <para>Requires <see cref="GetItemId"/> to be implemented.</para>
        /// </summary>
        protected void UpdateItemBuffered(object id, Action<TItem> updateAction, Func<TItem> getExistingNonBufferedItem)
        {
            BufferQueue.UpdateQueuedItemOrInsertUpdated(
                updateCondition: x => x.Id == id,
                updateAction: updateAction,
                updateBuffer: x => x.IsUpdate = true,
                getExistingNonBufferedItem: getExistingNonBufferedItem,
                addBuffered: x => InsertItemBuffered(x, id, isUpdate: true)
            );
        }

        /// <summary>
        /// Removes all matching items from the buffer queue.
        /// </summary>
        protected void RemoveFromBufferQueue(Func<BufferQueueItem, bool> filter)
            => BufferQueue.RemoveMatching(filter);

        /// <summary>
        /// Get all buffered new items not yet stored.
        /// </summary>
        protected IEnumerable<TItem> GetBufferedItems()
            => BufferQueue.GetBufferedItems().Select(x => x.Item);

        /// <summary>
        /// Called when the buffer is full or duration has been reached.
        /// </summary>
        protected virtual void OnBufferCallback(Queue<BufferQueueItem> items)
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
        protected abstract TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems);

        /// <summary>
        /// Skips waiting for the buffer and processes it at once.
        /// <para>Should not be needed for other things than unit testing.</para>
        /// </summary>
        public void ForceBufferCallback() => BufferQueue.ForceBufferCallback();
    }
}
