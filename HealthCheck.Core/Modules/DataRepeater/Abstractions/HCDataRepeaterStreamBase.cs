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
        #region IHCDataRepeaterStream Implementation
        /// <inheritdoc />
        public IHCDataRepeaterStreamItemStorage Storage { get; }

        /// <inheritdoc />
        public abstract string StreamDisplayName { get; }

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
        public virtual List<string> InitiallySelectedTags { get; }

        /// <inheritdoc />
        public virtual List<string> ManuallyAllowedTags { get; }

        /// <inheritdoc />
        public abstract List<IHCDataRepeaterStreamItemAction> Actions { get; }

        /// <inheritdoc />
        public abstract Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id);

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
        /// A stream of data that supports being modified and reprocessed.
        /// <para>Optional base class that strips away a bit of boilerplate code and adds some shortcuts.</para>
        /// </summary>
        public HCDataRepeaterStreamBase(IHCDataRepeaterStreamItemStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Store a new item. <see cref="AnalyzeItemAsync"/> has already been called on it, and any tags added.
        /// </summary>
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

            await Storage.StoreItemAsync(item, hint);
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
        public override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item)
            => AnalyzeItemAsync(item as TData);

        /// <summary>
        /// Analyze item for potential issues and apply suitable tags.
        /// </summary>
        protected abstract Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TData item);
    }
}
