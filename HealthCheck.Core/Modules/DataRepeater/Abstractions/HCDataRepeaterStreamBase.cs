using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Modules.DataRepeater.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
    /// </summary>
    public abstract class HCDataRepeaterStreamBase : IHCDataRepeaterStream
    {
        #region IHCDataRepeaterStream Implementation
        /// <inheritdoc />
        public abstract string StreamDisplayName { get; }

        /// <inheritdoc />
        public abstract string StreamGroupName { get; }

        /// <inheritdoc />
        public abstract string ItemIdDisplayName { get; }

        /// <inheritdoc />
        public virtual List<string> InitiallySelectedTags { get; }

        /// <inheritdoc />
        public virtual List<string> ManuallyAllowedTags { get; }

        /// <inheritdoc />
        public abstract List<IHCDataRepeaterStreamItemAction> Actions { get; }

        /// <inheritdoc />
        public abstract Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id, string itemId);

        /// <inheritdoc />
        public abstract Task DeleteItemAsync(Guid id, string itemId);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id, string itemId);

        /// <inheritdoc />
        public virtual async Task StoreItemAsync(IHCDataRepeaterStreamItem item, object hint = null)
        {
            if (AnalyzeOnStoreNew)
            {
                var analyticResult = await AnalyzeItemAsync(item);
                if (analyticResult.DontStore)
                {
                    return;
                }
                HCDataRepeaterUtils.ApplyChangesToItem(item, analyticResult);
            }

            await StoreNewItemAsync(item, hint);
        }

        /// <inheritdoc />
        public abstract Task UpdateItemAsync(IHCDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model);

        /// <inheritdoc />
        public virtual async Task SetAllowItemRetryAsync(Guid id, string itemId, bool allow)
        {
            var item = await GetItemAsync(id, itemId);
            if (item == null || item.AllowRetry == allow)
            {
                return;
            }

            item.AllowRetry = allow;
            await UpdateItemAsync(item);
        }

        /// <inheritdoc />
        public virtual async Task AddItemTagAsync(Guid id, string itemId, string tag)
        {
            var item = await GetItemAsync(id, itemId);
            if (item == null || item.Tags?.Contains(tag) == true)
            {
                return;
            }

            item.Tags ??= new HashSet<string>();
            item.Tags.Add(tag);
            await UpdateItemAsync(item);
        }

        /// <inheritdoc />
        public virtual async Task RemoveItemTagAsync(Guid id, string itemId, string tag)
        {
            var item = await GetItemAsync(id, itemId);
            if (item == null || item.Tags?.Contains(tag) != true)
            {
                return;
            }

            item.Tags ??= new HashSet<string>();
            item.Tags.Add(tag);
            await UpdateItemAsync(item);
        }

        /// <inheritdoc />
        public virtual async Task RemoveAllItemTagsAsync(Guid id, string itemId)
        {
            var item = await GetItemAsync(id, itemId);
            if (item == null || item.Tags?.Any() != true)
            {
                return;
            }

            item.Tags ??= new HashSet<string>();
            item.Tags.Clear();
            await UpdateItemAsync(item);
        }

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterRetryResult> RetryItemAsync(IHCDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item);
        #endregion

        /// <summary>
        /// If true, <see cref="AnalyzeItemAsync"/> is called before storing a new item.
        /// <para>Defaults to true</para>.
        /// </summary>
        public bool AnalyzeOnStoreNew { get; set; } = true;

        /// <summary>
        /// Store a new item. <see cref="AnalyzeItemAsync"/> has already been called on it, and any tags added.
        /// </summary>
        protected abstract Task StoreNewItemAsync(IHCDataRepeaterStreamItem item, object hint = null);
    }
}
