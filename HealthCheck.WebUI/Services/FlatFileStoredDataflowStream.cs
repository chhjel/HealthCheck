using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// A built in dataflow stream that stores and retrieves entries from a flatfile.
    /// </summary>
    public abstract class FlatFileStoredDataflowStream<TEntry, TEntryId> : IDataflowStream
        where TEntry : IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Id of the stream. Defaults to full typename.
        /// </summary>
        public virtual string Id => GetType().FullName;

        /// <summary>
        /// Name of the stream.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Description of the stream.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// True if the stream allows to filter by date.
        /// </summary>
        public virtual bool SupportsFilterByDate => true;

        /// <summary>
        /// Return true to enable the stream in the UI.
        /// <para>Defaults to true.</para>
        /// </summary>
        public Func<bool> IsVisible { get; set; } = () => true;

        /// <summary>
        /// Return true to allow insertion of new entries. If false <see cref="InsertEntries(IList{TEntry}, DateTime?)"/> will do nothing.
        /// <para>Defaults to true.</para>
        /// </summary>
        public Func<bool> AllowInsert { get; set; } = () => true;

        private bool AllowInsertSafe => AllowInsert == null || AllowInsert() == true;
        private bool IsVisibleSafe => IsVisible == null || IsVisible() == true;

        private SimpleDataStoreWithId<TEntry, TEntryId> Store { get; set; }
        private readonly Dictionary<string, DataFlowPropertyDisplayInfo> PropertyInfos = new Dictionary<string, DataFlowPropertyDisplayInfo>();

        /// <summary>
        /// A built in dataflow stream that stores and retrieves entries from a flatfile.
        /// </summary>
        public FlatFileStoredDataflowStream(
            string filepath,
            Func<TEntry, TEntryId> idSelector,
            Action<TEntry, TEntryId> idSetter,
            TimeSpan? maxEntryAge = null
        )
        {
            Store = new SimpleDataStoreWithId<TEntry, TEntryId>(
                filepath,
                serializer: new Func<TEntry, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, TEntry>((row) => JsonConvert.DeserializeObject<TEntry>(row)),
                idSelector: idSelector,
                idSetter: idSetter,
                nextIdFactory: (entries, e) => idSelector(e)
            );

            if (maxEntryAge != null)
            {
                var minimumCleanupInterval = TimeSpan.FromMinutes(30);
                Store.RetentionOptions = new StorageRetentionOptions<TEntry>(
                    (e) => e.InsertionTime ?? DateTime.MinValue,
                    maxAge: maxEntryAge.Value,
                    minimumCleanupInterval: (maxEntryAge.Value < minimumCleanupInterval) ? maxEntryAge.Value : minimumCleanupInterval,
                    delayFirstCleanup: false
                );
            }
        }

        /// <summary>
        /// Register display info about a property.
        /// </summary>
        public void RegisterPropertyDisplayInfo(DataFlowPropertyDisplayInfo info)
        {
            if (info == null) return;
            PropertyInfos[info.PropertyName] = info;
        }

        /// <summary>
        /// Store a single entry. If it already exists it will be updated.
        /// </summary>
        public TEntry InsertEntry(TEntry entry, DateTime? timestamp = null)
        {
            if (!AllowInsertSafe || entry == null) return entry;

            entry.InsertionTime = timestamp ?? DateTime.Now;
            entry = Store.InsertOrUpdateItem(entry);

            return entry;
        }

        /// <summary>
        /// Store multiple entries. The ones that already exists will be updated.
        /// </summary>
        public void InsertEntries(IList<TEntry> entries, DateTime? timestamp = null)
        {
            if (!AllowInsertSafe || entries == null) return;

            foreach (var entry in entries)
            {
                entry.InsertionTime = timestamp ?? DateTime.Now;
            }

            Store.InsertOrUpdateItems(entries);
        }

        /// <summary>
        /// Get filtered stored entries.
        /// </summary>
        public virtual async Task<IEnumerable<IDataflowEntry>> GetLatestStreamEntriesAsync(DataflowStreamFilter filter)
        {
            if (!IsVisibleSafe) return await Task.FromResult(Enumerable.Empty<IDataflowEntry>());

            var items = Store.GetEnumerable();

            if (filter.FromDate != null)
            {
                items = items.Where(x => x.InsertionTime >= filter.FromDate);
            }

            if (filter.ToDate != null)
            {
                items = items.Where(x => x.InsertionTime <= filter.ToDate);
            }

            items = await FilterEntries(filter, items);

            return items
                .Skip(filter.Skip)
                .Take(filter.Take)
                .Cast<IDataflowEntry>();
        }

        /// <summary>
        /// Optionally filter entries here.
        /// </summary>
        protected virtual async Task<IEnumerable<TEntry>> FilterEntries(DataflowStreamFilter filter, IEnumerable<TEntry> entries)
            => await Task.FromResult(entries).ConfigureAwait(false);

        /// <summary>
        /// Get any registered property infos.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataFlowPropertyDisplayInfo> GetEntryPropertiesInfo()
            => PropertyInfos.Select(x => x.Value).ToList();
    }
}
