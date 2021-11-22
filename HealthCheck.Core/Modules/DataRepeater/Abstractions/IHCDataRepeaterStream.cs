using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// A stream of data that supports being modified and reprocessed.
    /// </summary>
    public interface IHCDataRepeaterStream
    {
        /// <summary>
        /// Get the storage for the items of this stream.
        /// </summary>
        IHCDataRepeaterStreamItemStorage Storage { get; }

        /// <summary>
        /// Name of the stream.
        /// </summary>
        string StreamDisplayName { get; }

        /// <summary>
        /// What to call the items in this stream.
        /// </summary>
        string StreamItemsName { get; }

        /// <summary>
        /// Name of the stream group if any.
        /// </summary>
        string StreamGroupName { get; }

        /// <summary>
        /// Name of the item id.
        /// </summary>
        string ItemIdDisplayName { get; }

        /// <summary>
        /// Label on the retry-button.
        /// </summary>
        string RetryActionName { get; }

        /// <summary>
        /// Optional description of the retry action.
        /// </summary>
        string RetryDescription { get; }

        /// <summary>
        /// List of initially selected tags in the UI for this stream.
        /// </summary>
        List<string> InitiallySelectedTags { get; }

        /// <summary>
        /// List of tags that can be filtered on in the UI.
        /// </summary>
        List<string> FilterableTags { get; }

        /// <summary>
        /// Optional extra actions that can be executed on items.
        /// </summary>
        List<IHCDataRepeaterStreamItemAction> Actions { get; }

        /// <summary>
        /// Retry the given item.
        /// </summary>
        Task<HCDataRepeaterRetryResult> RetryItemAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Analyze item for potential issues and apply suitable tags.
        /// </summary>
        Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Optional extra details about an item to display in the UI.
        /// </summary>
        Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id);
    }
}
