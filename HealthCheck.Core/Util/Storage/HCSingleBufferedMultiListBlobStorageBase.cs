using HealthCheck.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util.Storage
{
    /// <summary>
    /// Base implementation for storing multiple lists by id in a blob container with cache and buffer.
    /// </summary>
    public abstract class HCSingleBufferedMultiListBlobStorageBase<TData, TItem, TId>
        : HCSingleBufferedBlobStorageBase<TData, TItem>
        where TData : HCSingleBufferedMultiListBlobStorageBase<TData, TItem, TId>.IBufferedBlobMultiListStorageData, new()
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
        protected HCSingleBufferedMultiListBlobStorageBase(IHCCache cache)
            : base(cache)
        {
        }

        /// <summary>
        /// Optionally limit the max number of different lists.
        /// </summary>
        public HCSingleBufferedMultiListBlobStorageBase<TData, TItem, TId> SetMaxListCount(int? maxListCount)
        {
            MaxListCount = maxListCount;
            return this;
        }

        /// <summary>
        /// Optionally limit the max number of latest items to store per list.
        /// </summary>
        public HCSingleBufferedMultiListBlobStorageBase<TData, TItem, TId> SetMaxItemCountPerList(int? maxItemCountPerList)
        {
            MaxItemCountPerList = maxItemCountPerList;
            return this;
        }

        /// <inheritdoc />
        protected override TData UpdateDataFromBuffer(TData data, Queue<BufferQueueItem> bufferedItems)
        {
            foreach(var item in bufferedItems)
            {
                var listId = (item.GroupId != null) ? (TId)item.GroupId : default;

                if (!data.Lists.ContainsKey(listId))
                {
                    var list = new List<TItem>() { item.Item };
                    data.Lists[listId] = list;
                }
                else
                {
                    data.Lists[listId].Add(item.Item);
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
        /// Get all items, buffered or stored.
        /// </summary>
        protected IEnumerable<TItem> GetItems(TId listId)
        {
            var bufferedItems = BufferQueue.GetBufferedItems().Where(x => x.GroupId == (listId as object));
            foreach (var item in bufferedItems)
            {
                yield return item.Item;
            }

            var data = GetBlobData();
            if (data != null)
            {
                if (data.Lists.TryGetValue(listId, out var list))
                {
                    foreach (var item in list)
                    {
                        yield return item;
                    }
                }
            }
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
