using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Modules.DataRepeater.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
    /// </summary>
    public abstract class TKDataRepeaterStreamBase : ITKDataRepeaterStream
    {
        /// <summary>
        /// Config for the default logic in <see cref="HandleAddedDuplicateItemAsync"/>.
        /// </summary>
        protected DefaultDuplicationLogicHandlingConfig DefaultDuplicationLogicConfig { get; set; } = new();

        #region ITKDataRepeaterStream Implementation
        /// <inheritdoc />
        public ITKDataRepeaterStreamItemStorage Storage { get; }

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
        public abstract List<ITKDataRepeaterStreamItemAction> Actions { get; }

        /// <inheritdoc />
        public abstract List<ITKDataRepeaterStreamItemBatchAction> BatchActions { get; }

        /// <inheritdoc />
        public abstract Task<TKDataRepeaterStreamItemDetails> GetItemDetailsAsync(ITKDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<TKDataRepeaterRetryResult> RetryItemAsync(ITKDataRepeaterStreamItem item);

        /// <inheritdoc />
        public abstract Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(ITKDataRepeaterStreamItem item, bool isManualAnalysis = false);

        /// <summary>
        /// If <see cref="ITKDataRepeaterStreamItemStorage.AddItemAsync"/> is called when an item with the same <see cref="ITKDataRepeaterStreamItem.ItemId"/> already exists, this method is called to handle the conflict.
        /// <para>Modify the items passed as parameters and return weather to update or discard them.</para>
        /// <para>Defaults to merging the new item into the old with some extra logic.</para>
        /// </summary>
        public virtual Task<TKDataRepeaterItemMergeConflictResult> HandleAddedDuplicateItemAsync(ITKDataRepeaterStreamItem existingItem, ITKDataRepeaterStreamItem newItem)
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

            var result = new TKDataRepeaterItemMergeConflictResult
            {
                NewItemAction = TKDataRepeaterItemMergeConflictResult.NewItemActionType.Ignore,
                OldItemAction = TKDataRepeaterItemMergeConflictResult.OldItemActionType.Update
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
        public TKDataRepeaterStreamBase(ITKDataRepeaterStreamItemStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Store a new item. If <paramref name="analyze"/> is enabled <see cref="AnalyzeItemAsync"/> is called first and any resulting changes applied.
        /// <para>Does not handle duplicates unless storage implementation does it, <see cref="ITKDataRepeaterService.AddStreamItemAsync{TStream}"/> can be used to insert with duplicate handling.</para>
        /// </summary>
        public virtual async Task AddItemAsync(ITKDataRepeaterStreamItem item, object hint = null, bool analyze = true)
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
                    TKDataRepeaterUtils.ApplyChangesToItem(item, analyticResult);
                }
            }

            await Storage.AddItemAsync(item, hint);
        }
    }

    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
    /// </summary>
    public abstract class TKDataRepeaterStreamBase<TData> : TKDataRepeaterStreamBase
        where TData : class, ITKDataRepeaterStreamItem
    {
        /// <summary>
        /// A stream of data that supports being modified and reprocessed.
        /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
        /// </summary>
        public TKDataRepeaterStreamBase(ITKDataRepeaterStreamItemStorage storage) : base(storage) { }

        /// <inheritdoc />
        public override Task<TKDataRepeaterRetryResult> RetryItemAsync(ITKDataRepeaterStreamItem item)
            => RetryItemAsync(item as TData);

        /// <summary>
        /// Retry the given item.
        /// </summary>
        protected abstract Task<TKDataRepeaterRetryResult> RetryItemAsync(TData item);

        /// <inheritdoc />
        public override Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(ITKDataRepeaterStreamItem item, bool isManualAnalysis = false)
            => AnalyzeItemAsync(item as TData, isManualAnalysis);

        /// <summary>
        /// Analyze item for potential issues and apply suitable tags.
        /// </summary>
        protected abstract Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TData item, bool isManualAnalysis = false);

        /// <inheritdoc />
        public override Task<TKDataRepeaterStreamItemDetails> GetItemDetailsAsync(ITKDataRepeaterStreamItem item)
            => GetItemDetailsAsync(item as TData);

        /// <summary>
        /// Optional extra details about an item to display in the UI.
        /// </summary>
        protected abstract Task<TKDataRepeaterStreamItemDetails> GetItemDetailsAsync(TData item);
    }
}
