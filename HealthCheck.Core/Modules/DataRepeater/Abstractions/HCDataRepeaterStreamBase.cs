using HealthCheck.Core.Modules.DataRepeater.Models;
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
                var analyticResult = await ApplyAnalysisAsync(item);
                if (analyticResult.DontStore)
                {
                    return;
                }
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
        public abstract Task<HCDataRepeaterActionResult> RetryItemAsync(IHCDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item);
        #endregion

        /// <summary>
        /// If true, <see cref="AnalyzeItemAsync"/> is called before storing a new item.
        /// <para>Defaults to true</para>.
        /// </summary>
        public bool AnalyzeOnStoreNew { get; set; } = true;

        #region Shortcuts
        /// <summary>
        /// Get item matching the given id.
        /// </summary>
        public async Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id) => await GetItemAsync(id, null);

        /// <summary>
        /// Delete item matching the given id.
        /// </summary>
        public async Task DeleteItemAsync(Guid id) => await DeleteItemAsync(id, null);

        /// <summary>
        /// Remove all tags from the item matching the given id.
        /// </summary>
        public async Task RemoveAllItemTagsAsync(Guid id) => await RemoveAllItemTagsAsync(id, null);

        /// <summary>
        /// Remove a single tag from the item matching the given id.
        /// </summary>
        public async Task RemoveItemTagAsync(Guid id, string tag) => await RemoveItemTagAsync(id, null, tag);

        /// <summary>
        /// Add a tag to the item matching the given id.
        /// </summary>
        public async Task AddItemTagAsync(Guid id, string tag) => await AddItemTagAsync(id, null, tag);

        /// <summary>
        /// Toggle allow retry on the item matching the given id.
        /// </summary>
        public async Task SetAllowItemRetryAsync(Guid id, bool allow) => await SetAllowItemRetryAsync(id, null, allow);
        #endregion

        /// <summary>
        /// Runs analysis and applies the result to the given item.
        /// </summary>
        protected virtual async Task<HCDataRepeaterItemAnalysisResult> ApplyAnalysisAsync(IHCDataRepeaterStreamItem item)
        {
            var analyticResult = await AnalyzeItemAsync(item);
            if (analyticResult == null)
            {
                return new HCDataRepeaterItemAnalysisResult();
            }

            if (analyticResult.DontStore)
            {
                return analyticResult;
            }

            item.Tags ??= new HashSet<string>();
            if (analyticResult?.TagsThatShouldExist?.Any() == true)
            {
                foreach (var tag in analyticResult.TagsThatShouldExist)
                {
                    item.Tags.Add(tag);
                }
            }

            if (analyticResult?.TagsThatShouldNotExist?.Any() == true)
            {
                foreach (var tag in analyticResult.TagsThatShouldNotExist)
                {
                    item.Tags.Remove(tag);
                }
            }

            if (analyticResult?.AllowRetry != null)
            {
                item.AllowRetry = analyticResult.AllowRetry.Value;
            }

            return analyticResult;
        }

        /// <summary>
        /// Store a new item. <see cref="AnalyzeItemAsync"/> has already been called on it, and any tags added.
        /// </summary>
        protected abstract Task StoreNewItemAsync(IHCDataRepeaterStreamItem item, object hint = null);
    }
}
