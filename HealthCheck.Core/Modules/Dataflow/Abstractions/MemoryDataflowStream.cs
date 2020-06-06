using HealthCheck.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// Stream type that stores the last n items in memory.
    /// </summary>
    public abstract class MemoryDataflowStream<TAccessRole, TEntry>
        : StoredDataflowStream<TAccessRole, TEntry>
        where TEntry : IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Stream type that stores the last n items in memory.
        /// </summary>
        public MemoryDataflowStream(
            int maxItemCount,
            Action<TEntry, Guid> idSetter,
            TimeSpan? maxDuration = null)
            : base(new MemoryDataflowStreamStore(maxItemCount, idSetter, maxDuration))
        {
        }

        internal class MemoryDataflowStreamStore : IDataStoreWithEntryId<TEntry>
        {
            private int MaxItemCount { get; set; }
            private TimeSpan? MaxDuration { get; set; }
            private Action<TEntry, Guid> IdSetter { get; set; }

            private List<TEntry> Items { get; set; } = new List<TEntry>();

            public MemoryDataflowStreamStore(
                int maxItemCount,
                Action<TEntry, Guid> idSetter,
                TimeSpan? maxDuration = null)
            {
                MaxItemCount = maxItemCount;
                MaxDuration = maxDuration;
                IdSetter = idSetter;
            }

            public IEnumerable<TEntry> GetEnumerable()
            {
                lock(Items)
                {
                    return Items.ToList();
                }
            }

            public TEntry InsertOrUpdateItem(TEntry entry, Func<TEntry, TEntry> update = null)
            {
                lock(Items)
                {
                    IdSetter(entry, Guid.NewGuid());
                    Items.Insert(0, entry);
                }
                Cleanup();
                return entry;
            }

            public void InsertOrUpdateItems(IEnumerable<TEntry> entries)
            {
                foreach(var entry in entries)
                {
                    IdSetter(entry, Guid.NewGuid());
                    Items.Insert(0, entry);
                }
                Cleanup();
            }

            private void Cleanup()
            {
                lock(Items)
                {
                    var itemsToRemove = Items.Count - MaxItemCount;
                    if (itemsToRemove > 0)
                    {
                        Items = Items.Take(MaxItemCount).ToList();
                    }

                    if (MaxDuration != null)
                    {
                        var threshold = DateTimeOffset.Now.Add(-MaxDuration.Value).ToUniversalTime();
                        for(int i=0; i<Items.Count; i++)
                        {
                            if (Items[i].InsertionTime?.ToUniversalTime() < threshold)
                            {
                                Items.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }
    }
}
