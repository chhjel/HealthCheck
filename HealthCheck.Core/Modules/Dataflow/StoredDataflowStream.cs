using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Core.Modules.Dataflow.DataFlowPropertyDisplayInfo;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// A built in dataflow stream that stores and retrieves entries from a given data store implementation.
    /// </summary>
    public abstract class StoredDataflowStream<TAccessRole, TEntry> : IDataflowStream<TAccessRole>
        where TEntry : IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Optionally set roles that have access to this stream.
        /// <para>Defaults to null, giving anyone with access to the dataflow page access.</para>
        /// </summary>
        public virtual Maybe<TAccessRole> RolesWithAccess { get; }

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
        /// Optional name of a <see cref="DateTime"/> property that will be used for grouping in frontend.
        /// <para>Defaults to 'InsertionTime' to match <see cref="IDataflowEntryWithInsertionTime.InsertionTime"/>.</para>
        /// </summary>
        public virtual string DateTimePropertyNameForUI { get; set; } = "InsertionTime";

        /// <summary>
        /// Return true to enable the stream in the UI.
        /// <para>Defaults to true.</para>
        /// </summary>
        public Func<bool> IsVisible { get; set; } = () => true;

        /// <summary>
        /// Return true to allow insertion of new entries. If false <see cref="InsertEntries(IEnumerable{TEntry}, DateTime?)"/> will do nothing.
        /// <para>Defaults to true.</para>
        /// </summary>
        public Func<bool> AllowInsert { get; set; } = () => true;

        private bool AllowInsertSafe => AllowInsert == null || AllowInsert() == true;
        private bool IsVisibleSafe => IsVisible == null || IsVisible() == true;

        /// <summary>
        /// Implementation that stores the stream entries.
        /// </summary>
        protected IDataStoreWithEntryId<TEntry> Store { get; set; }

        private readonly Dictionary<string, DataFlowPropertyDisplayInfo> PropertyInfos = new Dictionary<string, DataFlowPropertyDisplayInfo>();
        private List<Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>>
            InternalPropertyFilters = new List<Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>>();

        /// <summary>
        /// A built in dataflow stream that stores and retrieves entries from a flatfile.
        /// </summary>
        public StoredDataflowStream(IDataStoreWithEntryId<TEntry> dataStore)
        {
            Store = dataStore;
        }

        /// <summary>
        /// Register display info about a property. UIOrder will be set to the invoke count of this method instance.
        /// </summary>
        public DataFlowPropertyDisplayInfo ConfigureProperty(string propertyName, string displayName,
            DataFlowPropertyUIHint? uiHint = null, DataFlowPropertyUIVisibilityOption? visibility = null)
            => ConfigureProperty(propertyName, uiHint, visibility).SetDisplayName(displayName);

        /// <summary>
        /// Register display info about a property. UIOrder will be set to the invoke count of this method instance.
        /// </summary>
        public DataFlowPropertyDisplayInfo ConfigureProperty(string propertyName,
            DataFlowPropertyUIHint? uiHint = null, DataFlowPropertyUIVisibilityOption? visibility = null)
        {
            var info = PropertyInfos.ContainsKey(propertyName) 
                ? PropertyInfos[propertyName]
                : PropertyInfos[propertyName] = new DataFlowPropertyDisplayInfo(propertyName).SetUIOrder(RegisterPropertyDisplayInfoInvokeCount++);

            if (uiHint != null)
            {
                info.SetUIHint(uiHint.Value);
            }
            if (visibility != null)
            {
                info.SetVisibility(visibility.Value);
            }

            return info;
        }
        private int RegisterPropertyDisplayInfoInvokeCount = 0;

        /// <summary>
        /// Register display info about a property.
        /// </summary>
        public void ConfigureProperty(DataFlowPropertyDisplayInfo info)
        {
            if (info == null) return;
            PropertyInfos[info.PropertyName] = info;
        }

        /// <summary>
        /// Register display info about multiple properties.
        /// <para>UI order will be set according to the order of the items.</para>
        /// </summary>
        public void ConfigureProperties(params DataFlowPropertyDisplayInfo[] infos)
        {
            if (infos == null || !infos.Any()) return;
            var order = 0;
            foreach (var info in infos)
            {
                info.UIOrder = order++;
                PropertyInfos[info.PropertyName] = info;
            }
        }

        /// <summary>
        /// Store a single entry on a new thread. If it already exists it will be updated.
        /// <para>Catches and ignores exceptions.</para>
        /// </summary>
        public void TryFireAndForgetInsertEntry(TEntry entry, DateTime? timestamp = null)
        {
            Task.Run(() => { try { InsertEntry(entry, timestamp); } catch (Exception) { } });
        }

        /// <summary>
        /// Store multiple entries on a new thread. The ones that already exists will be updated.
        /// <para>Catches and ignores exceptions.</para>
        /// </summary>
        public void TryFireAndForgetInsertEntries(IList<TEntry> entries, DateTime? timestamp = null)
        {
            Task.Run(() => { try { InsertEntries(entries, timestamp); } catch (Exception) { } });
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
        public void InsertEntries(IEnumerable<TEntry> entries, DateTime? timestamp = null)
        {
            if (!AllowInsertSafe || entries == null) return;

            var entriesList = entries.ToList();
            for (var i=0; i< entriesList.Count; i++)
            {
                entriesList[i].InsertionTime = timestamp ?? DateTime.Now;
            }

            Store.InsertOrUpdateItems(entriesList);
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

            items = items.OrderByDescending(x => x.InsertionTime);

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
        {
            foreach(var internalFilter in InternalPropertyFilters)
            {
                entries = internalFilter(entries, filter);
            }
            return await Task.FromResult(entries).ConfigureAwait(false);
        }

        /// <summary>
        /// Get any registered property infos.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataFlowPropertyDisplayInfo> GetEntryPropertiesInfo()
            => PropertyInfos.Select(x => x.Value).ToList();

        /// <summary>
        /// Create filter for suitable property types automatically.
        /// </summary>
        public void AutoCreateFilters<TItem>(
                IEnumerable<string> memberNames = null,
                IEnumerable<string> excludedMemberNames = null)
        {
            var filters = GenericDataflowStreamObject.CreateAutoFilters<TItem>(memberNames, excludedMemberNames)
                .Where(x => x.Filter is Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>);

            foreach(var filter in filters)
            {
                ConfigureProperty(filter.MemberName).SetFilterable();
                InternalPropertyFilters.Add((Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>)filter.Filter);
            }
        }
    }
}
