using HealthCheck.Core.Abstractions;
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
