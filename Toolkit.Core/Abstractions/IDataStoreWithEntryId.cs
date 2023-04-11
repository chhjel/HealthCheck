using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Abstractions
{
    /// <summary>
    /// A generic object storage.
    /// </summary>
    public interface IDataStoreWithEntryId<TEntry> : IDataStore<TEntry>
    {
        /// <summary>
        /// Store the given item. Update it if it already exists.
        /// </summary>
        TEntry InsertOrUpdateItem(TEntry item, Func<TEntry, TEntry> update = null);

        /// <summary>
        /// Store the given item. Update any that already exists.
        /// </summary>
        void InsertOrUpdateItems(IEnumerable<TEntry> items);
    }
}
