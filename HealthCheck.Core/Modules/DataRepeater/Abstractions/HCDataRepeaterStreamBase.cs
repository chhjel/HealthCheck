using HealthCheck.Core.Modules.DataRepeater.Extensions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Modules.DataRepeater.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
    /// </summary>
    public abstract class HCDataRepeaterStreamBase : IHCDataRepeaterStream
    {
        /// <summary>
        /// Config for the default logic in <see cref="HandleAddedDuplicateItemAsync"/>.
        /// </summary>
        protected DefaultDuplicationLogicHandlingConfig DefaultDuplicationLogicConfig { get; set; } = new();

        #region IHCDataRepeaterStream Implementation
        /// <inheritdoc />
        public IHCDataRepeaterStreamItemStorage Storage { get; }

        /// <inheritdoc />
        public abstract string StreamDisplayName { get; }

        /// <inheritdoc />
        public virtual string StreamDescription { get; }

        /// <inheritdoc />
        public abstract string StreamItemsName { get; }

        /// <inheritdoc />
        public abstract string StreamGroupName { get; }

        /// <inheritdoc />
        public abstract string ItemIdDisplayName { get; }

        /// <inheritdoc />
        public abstract string RetryActionName { get; }

        /// <inheritdoc />
        public abstract string RetryDescription { get; }

        /// <inheritdoc />
        public abstract List<string> InitiallySelectedTags { get; }

        /// <inheritdoc />
        public abstract List<string> FilterableTags { get; }

        /// <inheritdoc />
        public virtual object AllowedAccessRoles { get; set; }

        /// <inheritdoc />
        public virtual List<string> Categories { get; } = new();

        /// <inheritdoc />
        public virtual bool ManualAnalyzeEnabled { get; } = true;

        /// <inheritdoc />
        public virtual string AnalyzeActionName { get; } = "Analyze";

        /// <inheritdoc />
        public abstract List<IHCDataRepeaterStreamItemAction> Actions { get; }

        /// <inheritdoc />
        public abstract List<IHCDataRepeaterStreamItemBatchAction> BatchActions { get; }

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(IHCDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterRetryResult> RetryItemAsync(IHCDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item, bool isManualAnalysis = false);

        /// <summary>
        /// If <see cref="IHCDataRepeaterStreamItemStorage.AddItemAsync"/> is called when an item with the same <see cref="IHCDataRepeaterStreamItem.ItemId"/> already exists, this method is called to handle the conflict.
        /// <para>Modify the items passed as parameters and return weather to update or discard them.</para>
        /// <para>Defaults to merging the new item into the old with some extra logic.</para>
        /// </summary>
        public virtual Task<HCDataRepeaterItemMergeConflictResult> HandleAddedDuplicateItemAsync(IHCDataRepeaterStreamItem existingItem, IHCDataRepeaterStreamItem newItem)
        {
            var config = DefaultDuplicationLogicConfig ?? new();
            if (config.RemoveExistingTags == true)
            {
                existingItem.Tags.Clear();
            }

            foreach (var tag in newItem.Tags)
            {
                existingItem.Tags.Add(tag);
            }

            existingItem.AllowRetry = newItem.AllowRetry;
            existingItem.ExpirationTime = newItem.ExpirationTime ?? existingItem.ExpirationTime;
            existingItem.SerializedData = newItem.SerializedData;
            existingItem.Summary = !string.IsNullOrWhiteSpace(newItem.Summary) ? newItem.Summary : existingItem.Summary;
            existingItem.ForcedStatus = newItem.ForcedStatus != null ? newItem.ForcedStatus : existingItem.ForcedStatus;

            // First error
            var firstError = existingItem.FirstError;
            if (string.IsNullOrWhiteSpace(firstError) && !string.IsNullOrWhiteSpace(newItem.Error))
            {
                firstError = newItem.Error;
            }
            if (string.IsNullOrWhiteSpace(existingItem.FirstError) && !string.IsNullOrWhiteSpace(firstError))
            {
                existingItem.FirstErrorAt = DateTimeOffset.Now;
            }
            existingItem.FirstError = string.IsNullOrWhiteSpace(existingItem.FirstError) ? newItem.FirstError : existingItem.FirstError;

            // Latest error
            existingItem.Error = !string.IsNullOrWhiteSpace(newItem.Error) ? newItem.Error : existingItem.Error;
            if (!string.IsNullOrWhiteSpace(existingItem.Error))
            {
                existingItem.LastErrorAt = DateTimeOffset.Now;
            }

            if (!string.IsNullOrWhiteSpace(config.LogMessage))
            {
                existingItem.AddLogMessage(config.LogMessage);
            }

            var result = new HCDataRepeaterItemMergeConflictResult
            {
                NewItemAction = HCDataRepeaterItemMergeConflictResult.NewItemActionType.Ignore,
                OldItemAction = HCDataRepeaterItemMergeConflictResult.OldItemActionType.Update
            };
            return Task.FromResult(result);
        }

        /// <summary>
        /// Config for the default logic in <see cref="HandleAddedDuplicateItemAsync"/>.
        /// </summary>
        public class DefaultDuplicationLogicHandlingConfig
        {
            /// <summary>
            /// If set to true, any existing tags will be removed before adding any new ones.
            /// </summary>
            public bool RemoveExistingTags { get; set; }

            /// <summary>
            /// Log message to append.
            /// <para>Defaults to: "Merged with new details."</para>
            /// <para>Set to empty or null to not log any message.</para>
            /// </summary>
            public string LogMessage { get; set; } = "Merged with new details.";
        }
        #endregion

        /// <summary>
        /// A stream of data that supports being modified and reprocessed.
        /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
        /// </summary>
        public HCDataRepeaterStreamBase(IHCDataRepeaterStreamItemStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Store a new item. If <paramref name="analyze"/> is enabled <see cref="AnalyzeItemAsync"/> is called first and any resulting changes applied.
        /// <para>Does not handle duplicates unless storage implementation does it, <see cref="IHCDataRepeaterService.AddStreamItemAsync{TStream}"/> can be used to insert with duplicate handling.</para>
        /// </summary>
        public virtual async Task AddItemAsync(IHCDataRepeaterStreamItem item, object hint = null, bool analyze = true)
        {
            if (item == null) return;
            item.Log ??= new();

            if (analyze)
            {
                var analyticResult = await AnalyzeItemAsync(item, isManualAnalysis: false);
                if (analyticResult != null)
                {
                    if (analyticResult.DontStore)
                    {
                        return;
                    }
                    HCDataRepeaterUtils.ApplyChangesToItem(item, analyticResult);
                }
            }

            await Storage.AddItemAsync(item, hint);
        }
    }

    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
    /// </summary>
    public abstract class HCDataRepeaterStreamBase<TData> : HCDataRepeaterStreamBase
        where TData : class, IHCDataRepeaterStreamItem
    {
        /// <summary>
        /// A stream of data that supports being modified and reprocessed.
        /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
        /// </summary>
        public HCDataRepeaterStreamBase(IHCDataRepeaterStreamItemStorage storage) : base(storage) { }

        /// <inheritdoc />
        public override Task<HCDataRepeaterRetryResult> RetryItemAsync(IHCDataRepeaterStreamItem item)
            => RetryItemAsync(item as TData);

        /// <summary>
        /// Retry the given item.
        /// </summary>
        protected abstract Task<HCDataRepeaterRetryResult> RetryItemAsync(TData item);

        /// <inheritdoc />
        public override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item, bool isManualAnalysis = false)
            => AnalyzeItemAsync(item as TData, isManualAnalysis);

        /// <summary>
        /// Analyze item for potential issues and apply suitable tags.
        /// </summary>
        protected abstract Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TData item, bool isManualAnalysis = false);

        /// <inheritdoc />
        public override Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(IHCDataRepeaterStreamItem item)
            => GetItemDetailsAsync(item as TData);

        /// <summary>
        /// Optional extra details about an item to display in the UI.
        /// </summary>
        protected abstract Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(TData item);
    }
}
