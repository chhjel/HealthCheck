using EPiServer.Framework.Blobs;
using HealthCheck.Core.Util.Collections;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace HealthCheck.Episerver.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCEpiserverSingleBufferedBlobStorageBase<TData, TItem> : HCEpiserverSingleBlobStorageBase<TData>
        where TData : new()
    {
        /// <summary>
        /// Max duration to buffer data in memory for before writing to blob storage.
        /// <para>Defaults to 10 seconds.</para>
        /// </summary>
        public virtual TimeSpan BlobUpdateBufferDuration { get => BufferQueue.Delay; set => BufferQueue.Delay = value; }

        /// <summary>
        /// Max item count 
        /// <para>Defaults to 100.</para>
        /// </summary>
        public virtual int MaxBufferSize { get => BufferQueue.QueueSizeLimit; set => BufferQueue.QueueSizeLimit = value; }

        /// <summary>
        /// The buffered queue of items to add.
        /// </summary>
        protected readonly DelayedBufferQueue<TItem> BufferQueue;

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCEpiserverSingleBufferedBlobStorageBase(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
            BufferQueue = new DelayedBufferQueue<TItem>(OnBufferCallback, TimeSpan.FromSeconds(10), 100);
        }

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemBuffered(TItem item) => BufferQueue.Add(item);

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemsBuffered(IEnumerable<TItem> items) => BufferQueue.Add(items);

        /// <summary>
        /// Called when the buffer is full or duration has been reached.
        /// </summary>
        protected virtual void OnBufferCallback(Queue<TItem> items)
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
        protected abstract TData UpdateDataFromBuffer(TData data, Queue<TItem> bufferedItems);
    }
}
