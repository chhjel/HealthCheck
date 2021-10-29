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
        /// <para>Defaults to 100.</para>
        /// </summary>
        public virtual int MaxBufferSize { get => BufferQueue.QueueSizeLimit; set => BufferQueue.QueueSizeLimit = value; }

        /// <summary>
        /// The buffered queue of items to add.
        /// </summary>
        private readonly DelayedBufferQueue<BufferQueueItem> BufferQueue;

        /// <summary>
        /// Wrapper type for buffered items.
        /// </summary>
        protected struct BufferQueueItem
        {
            /// <summary>
            /// Id used in some implementations.
            /// </summary>
            public object Id { get; set; }

            /// <summary>
            /// Buffered item.
            /// </summary>
            public TItem ItemToInsert { get; set; }

            /// <summary>
            /// True if <see cref="ItemToInsert"/> is set.
            /// </summary>
            public bool IsInsert => ItemToInsert != null;

            /// <summary>
            /// Action to perform on existing item.
            /// </summary>
            public Action<TItem> UpdateAction { get; set; }

            /// <summary>
            /// True if <see cref="UpdateAction"/> is set.
            /// </summary>
            public bool IsUpdate => UpdateAction != null;
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
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemBuffered(TItem item, object id = null) => BufferQueue.Add(new BufferQueueItem { Id = id, ItemToInsert = item });

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemsBuffered(IEnumerable<TItem> items, object id = null)
            => BufferQueue.Add(items.Select(x => new BufferQueueItem { Id = id, ItemToInsert = x }));

        /// <summary>
        /// Queues up an update to an item and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void UpdateItemBuffered(object id, Action<TItem> updateAction)
            => BufferQueue.Add(new BufferQueueItem { Id = id, UpdateAction = updateAction });

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
    }
}
