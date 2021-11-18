using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// <para>Optional base class that strips away a bit of boilerplate code.</para>
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
        public virtual async Task StoreItemAsync(IHCDataRepeaterStreamItem item, object hint = null)
        {
            var analyticResult = AnalyzeOnStoreNew ? await AnalyzeItemAsync(item) : null;

            item.Tags ??= new HashSet<string>();
            if (analyticResult?.TagsThatShouldBeApplied?.Any() == true)
            {
                foreach(var tag in analyticResult.TagsThatShouldBeApplied)
                {
                    item.Tags.Add(tag);
                }
            }

            if (analyticResult?.AllowRetry != null)
            {
                item.AllowRetry = analyticResult.AllowRetry.Value;
            }

            await StoreNewItemAsync(item, hint);
        }

        /// <inheritdoc />
        public abstract Task UpdateItemAsync(IHCDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task DeleteItemAsync(Guid id, string itemId);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id, string itemId);

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
        public virtual async Task<HCDataRepeaterActionResult> RetryItemAsync(IHCDataRepeaterStreamItem item)
        {
            var result = await ProcessItemRetryAsync(item);

            var shouldSave = false;
            if (result?.AllowRetry != null && item.AllowRetry != result.AllowRetry)
            {
                shouldSave = true;
                item.AllowRetry = result.AllowRetry.Value;
            }
            if (result?.TagsThatShouldBeApplied?.Any(x => item.Tags?.Contains(x) == false) == true)
            {
                shouldSave = true;
                item.Tags ??= new HashSet<string>();
                foreach (var tag in result.TagsThatShouldBeApplied)
                {
                    item.Tags.Add(tag);
                }
            }

            if (shouldSave)
            {
                await UpdateItemAsync(item);
            }

            return result;
        }

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

        /// <summary>
        /// Attempts to retry an item.
        /// </summary>
        protected abstract Task<HCDataRepeaterActionResultExt> ProcessItemRetryAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Extended model used by <see cref="HCDataRepeaterStreamBase"/>
        /// </summary>
        public class HCDataRepeaterActionResultExt : HCDataRepeaterActionResult
        {
            /// <summary>
            /// Tags that will be applied if missing.
            /// </summary>
            public IEnumerable<string> TagsThatShouldBeApplied { get; set; }

            /// <summary>
            /// Optionally configure if further retrying should be allowed.
            /// </summary>
            public bool? AllowRetry { get; set; }
        }
    }
}
