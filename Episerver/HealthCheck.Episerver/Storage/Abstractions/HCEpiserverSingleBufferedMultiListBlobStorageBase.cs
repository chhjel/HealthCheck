using EPiServer.Framework.Blobs;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage.Abstractions
{
    /// <summary>
    /// Base implementation for storing multiple lists by id in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCEpiserverSingleBufferedMultiListBlobStorageBase<TData, TItem, TId> 
        : HCEpiserverSingleBufferedBlobStorageBase<TData, TItem>
        where TData : HCEpiserverSingleBufferedMultiListBlobStorageBase<TData, TItem, TId>.IBufferedBlobMultiListStorageData, new()
    {
        /// <summary>
        /// Optionally limit the max number of different lists.
        /// <para>Oldest lists will be discarded.</para>
        /// </summary>
        public virtual int? MaxListCount { get; set; }

        /// <summary>
        /// Optionally limit the max number of latest items to store per list.
        /// <para>Oldest items will be discarded.</para>
        /// <para>Defaults to 1000.</para>
        /// </summary>
        public virtual int? MaxItemCountPerList { get; set; } = 1000;

        /// <summary>
        /// Base implementation for storing a single object in a blob container with cache.
        /// </summary>
        protected HCEpiserverSingleBufferedMultiListBlobStorageBase(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
        }

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems)
        {
            foreach(var item in bufferedItems)
            {
                var id = (item.Id != null) ? (TId)item.Id : default;

                if (!data.Lists.ContainsKey(id))
                {
                    var list = new List<TItem>() { item.Item };
                    data.Lists[id] = list;
                }
                else
                {
                    data.Lists[id].Add(item.Item);
                }
            }

            if (MaxListCount != null && data.Lists.Count > MaxListCount)
            {
                var skipCount = data.Lists.Count - MaxListCount.Value;
                data.Lists = data.Lists.Skip(skipCount).ToDictionary(x => x.Key, x => x.Value);
            }

            if (MaxItemCountPerList != null)
            {
                foreach(var list in data.Lists.Values)
                {
                    if (list.Count > MaxItemCountPerList)
                    {
                        var skipCount = list.Count - MaxItemCountPerList.Value;
                        list.RemoveRange(0, skipCount);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Stored data model.
        /// </summary>
        public interface IBufferedBlobMultiListStorageData
        {
            /// <summary>
            /// Stored lists.
            /// </summary>
            Dictionary<TId, List<TItem>> Lists { get; set; }
        }
    }
}
