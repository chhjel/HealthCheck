using EPiServer.Framework.Blobs;
using HealthCheck.Core.Util.Collections;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Abstractions
{
    /// <summary>
    /// Base implementation for storing a single object in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCEpiserverSingleBufferedBlobStorageBase<TData, TItem> : HCEpiserverSingleBlobStorageBase<TData>
        where TData : HCEpiserverSingleBufferedBlobStorageBase<TData, TItem>.IBufferedBlobStorageData, new()
    {
        /// <summary>
        /// Max duration to buffer data in memory for before writing to blob storage.
        /// <para>Defaults to 30 seconds.</para>
        /// </summary>
        public virtual TimeSpan BlobUpdateBufferDuration { get => _bufferStack.Delay; set => _bufferStack.Delay = value; }

        /// <summary>
        /// Max item count 
        /// <para>Defaults to 100.</para>
        /// </summary>
        public int MaxBufferSize { get => _bufferStack.QueueSizeLimit; set => _bufferStack.QueueSizeLimit = value; }

        /// <summary>
        /// Optionally limit the max number of latest items to store.
        /// </summary>
        public int? MaxItemCount { get; set; }

        private readonly DelayedBufferQueue<TItem> _bufferStack;

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCEpiserverSingleBufferedBlobStorageBase(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
            _bufferStack = new DelayedBufferQueue<TItem>(OnBufferCallback, TimeSpan.FromSeconds(30), 100);
        }

        /// <summary>
        /// Queues up items and calls <see cref="OnBufferCallback"/> after a delay or when max count is reached.
        /// </summary>
        protected void InsertItemBuffered(TItem item) => _bufferStack.Add(item);

        /// <summary>
        /// Performs the actual save and optionally limits count if <see cref="MaxItemCount"/> has been set.
        /// <para>Called when the buffer is full or duration has been reached.</para>
        /// </summary>
        protected virtual void OnBufferCallback(Queue<TItem> items)
        {
            var data = GetBlobData();

            data.Items.AddRange(items);

            if (MaxItemCount != null && data.Items.Count > MaxItemCount)
            {
                var skipCount = data.Items.Count - MaxItemCount.Value;
                data.Items = data.Items.Skip(skipCount).ToList();
            }

            SaveBlobData(data);
        }

        /// <summary>
        /// Stored data model.
        /// </summary>
        public interface IBufferedBlobStorageData
        {
            /// <summary>
            /// Stored items.
            /// </summary>
            List<TItem> Items { get; set; }
        }
    }
}
