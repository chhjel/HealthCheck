using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// A built in dataflow stream that stores and retrieves entries from a given data store implementation.
    /// </summary>
    public abstract class StoredDataflowStream<TAccessRole, TEntry> : DataflowStreamBase<TAccessRole, TEntry>
        where TEntry : IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Return true to allow insertion of new entries. If false <see cref="InsertEntries(IEnumerable{TEntry}, DateTimeOffset?)"/> will do nothing.
        /// <para>Defaults to true.</para>
        /// </summary>
        public Func<bool> AllowInsert { get; set; } = () => true;
        private bool AllowInsertSafe => AllowInsert == null || AllowInsert();

        /// <summary>
        /// Implementation that stores the stream entries.
        /// </summary>
        protected IDataStoreWithEntryId<TEntry> Store { get; set; }

        /// <summary>
        /// A built in dataflow stream that stores and retrieves entries from a flatfile.
        /// </summary>
        protected StoredDataflowStream(IDataStoreWithEntryId<TEntry> dataStore)
        {
            Store = dataStore;
        }

        /// <summary>
        /// Store a single entry on a new thread. If it already exists it will be updated.
        /// <para>Catches and ignores exceptions.</para>
        /// </summary>
        public void TryFireAndForgetInsertEntry(TEntry entry, DateTimeOffset? timestamp = null)
        {
            Task.Run(() => { try { InsertEntry(entry, timestamp); } catch (Exception) { /* Ignore any exceptions here. */ } });
        }

        /// <summary>
        /// Store multiple entries on a new thread. The ones that already exists will be updated.
        /// <para>Catches and ignores exceptions.</para>
        /// </summary>
        public void TryFireAndForgetInsertEntries(IList<TEntry> entries, DateTimeOffset? timestamp = null)
        {
            Task.Run(() => { try { InsertEntries(entries, timestamp); } catch (Exception) { /* Ignore any exceptions here. */ } });
        }

        /// <summary>
        /// Store a single entry. If it already exists it will be updated.
        /// </summary>
        public TEntry InsertEntry(TEntry entry, DateTimeOffset? timestamp = null)
        {
            if (!AllowInsertSafe || entry == null) return entry;

            entry.InsertionTime = timestamp ?? DateTimeOffset.Now;
            entry = Store.InsertOrUpdateItem(entry);

            return entry;
        }

        /// <summary>
        /// Store multiple entries. The ones that already exists will be updated.
        /// </summary>
        public void InsertEntries(IEnumerable<TEntry> entries, DateTimeOffset? timestamp = null)
        {
            if (!AllowInsertSafe || entries == null) return;

            var entriesList = entries.ToList();
            for (var i=0; i< entriesList.Count; i++)
            {
                entriesList[i].InsertionTime = timestamp ?? DateTimeOffset.Now;
            }

            Store.InsertOrUpdateItems(entriesList);
        }

        /// <summary>
        /// Get entries. They will automatically be filtered on date and skipped/taken afterwards.
        /// </summary>
        protected override async Task<IEnumerable<TEntry>> GetStreamEntries(DataflowStreamFilter filter)
            => await Task.FromResult(Store.GetEnumerable());
    }
}
