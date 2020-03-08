﻿using HealthCheck.Core.Modules.Dataflow;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// A generic object storage.
    /// </summary>
    public interface IDataStoreWithEntryId<TEntry> : IDataStore<TEntry>
    {
        /// <summary>
        /// Store the given item. Update it if it already exists.
        /// </summary>
        TEntry InsertOrUpdateItem(TEntry entry);

        /// <summary>
        /// Store the given item. Update any that already exists.
        /// </summary>
        void InsertOrUpdateItems(IEnumerable<TEntry> entries);
    }
}