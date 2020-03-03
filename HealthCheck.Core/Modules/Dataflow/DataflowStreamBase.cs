using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Core.Modules.Dataflow.DataFlowPropertyDisplayInfo;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Stream type that stores the last n items in memory.
    /// </summary>
    public abstract class DataflowStreamBase<TAccessRole, TEntry> : IDataflowStream<TAccessRole>
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
        /// Optionally group the stream within the given group name.
        /// </summary>
        public abstract string GroupName { get; }

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

        private bool IsVisibleSafe => IsVisible == null || IsVisible() == true;

        private readonly Dictionary<string, DataFlowPropertyDisplayInfo> PropertyInfos = new Dictionary<string, DataFlowPropertyDisplayInfo>();
        private List<Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>>
            InternalPropertyFilters = new List<Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>>();

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
        /// Get filtered stored entries.
        /// </summary>
        public async Task<IEnumerable<IDataflowEntry>> GetLatestStreamEntriesAsync(DataflowStreamFilter filter)
        {
            if (!IsVisibleSafe) return await Task.FromResult(Enumerable.Empty<IDataflowEntry>());

            var items = await GetStreamEntries(filter);

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
        /// Get entries. They will automatically be filtered on date and skipped/taken afterwards.
        /// </summary>
        protected abstract Task<IEnumerable<TEntry>> GetStreamEntries(DataflowStreamFilter filter);

        /// <summary>
        /// Optionally filter entries here.
        /// </summary>
        protected virtual async Task<IEnumerable<TEntry>> FilterEntries(DataflowStreamFilter filter, IEnumerable<TEntry> entries)
        {
            foreach (var internalFilter in InternalPropertyFilters)
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

            foreach (var filter in filters)
            {
                ConfigureProperty(filter.MemberName).SetFilterable();
                InternalPropertyFilters.Add((Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>)filter.Filter);
            }
        }
    }
}
