using EPiServer.Framework.Blobs;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing a list in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCEpiserverSingleBufferedListBlobStorageBase<TData, TItem> : HCEpiserverSingleBufferedBlobStorageBase<TData, TItem>
        where TData : HCEpiserverSingleBufferedListBlobStorageBase<TData, TItem>.IBufferedBlobListStorageData, new()
    {
        /// <summary>
        /// Optionally limit the max number of latest items to store.
        /// </summary>
        public virtual int? MaxItemCount { get; set; }

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCEpiserverSingleBufferedListBlobStorageBase(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
        }

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems)
        {
            data.Items.AddRange(bufferedItems.Select(x => x.Item));

            if (MaxItemCount != null && data.Items.Count > MaxItemCount)
            {
                var skipCount = data.Items.Count - MaxItemCount.Value;
                data.Items.RemoveRange(0, skipCount);
            }

            return data;
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
