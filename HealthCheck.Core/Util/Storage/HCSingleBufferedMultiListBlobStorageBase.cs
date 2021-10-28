﻿using HealthCheck.Core.Abstractions;
using System;
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
                if (item.IsUpdate) throw new NotImplementedException($"{GetType().Name} does not have support for buffered updates yet.");

                var id = (item.Id != null) ? (TId)item.Id : default;

                if (!data.Lists.ContainsKey(id))
                {
                    var list = new List<TItem>() { item.ItemToInsert };
                    data.Lists[id] = list;
                }
                else
                {
                    data.Lists[id].Add(item.ItemToInsert);
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
