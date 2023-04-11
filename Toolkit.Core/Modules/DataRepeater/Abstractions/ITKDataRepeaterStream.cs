using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

/// <summary>
/// A stream of data that supports being modified and reprocessed.
/// </summary>
public interface ITKDataRepeaterStream
{
    /// <summary>
    /// Get the storage for the items of this stream.
    /// </summary>
    ITKDataRepeaterStreamItemStorage Storage { get; }

    /// <summary>
    /// Name of the stream.
    /// </summary>
    string StreamDisplayName { get; }

    /// <summary>
    /// Optional description of the stream. Supports HTML.
    /// </summary>
    string StreamDescription { get; }

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
    /// Show the manual analyzation button if the request has access to it for this stream.
    /// </summary>
    bool ManualAnalyzeEnabled { get; }

    /// <summary>
    /// Label on the analyze-button.
    /// </summary>
    string AnalyzeActionName { get; }

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
    /// Optional access roles that can access this stream.
    /// <para>Must be a flags enum of the same type as the one used on the toolkit controller.</para>
    /// </summary>
    object AllowedAccessRoles { get; }

    /// <summary>
    /// Optional categories this stream belongs to.
    /// <para>Can be used for more granular access configuration.</para>
    /// </summary>
    List<string> Categories { get; }

    /// <summary>
    /// Optional extra actions that can be executed on single items.
    /// </summary>
    List<ITKDataRepeaterStreamItemAction> Actions { get; }

    /// <summary>
    /// Optional extra actions that can be executed on all items.
    /// </summary>
    List<ITKDataRepeaterStreamItemBatchAction> BatchActions { get; }

    /// <summary>
    /// Retry the given item.
    /// </summary>
    Task<TKDataRepeaterRetryResult> RetryItemAsync(ITKDataRepeaterStreamItem item);

    /// <summary>
    /// Analyze item for potential issues and apply suitable tags.
    /// </summary>
    Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(ITKDataRepeaterStreamItem item, bool isManualAnalysis = false);

    /// <summary>
    /// Optional extra details about an item to display in the UI.
    /// </summary>
    Task<TKDataRepeaterStreamItemDetails> GetItemDetailsAsync(ITKDataRepeaterStreamItem item);

    /// <summary>
    /// If <see cref="ITKDataRepeaterStreamItemStorage.AddItemAsync"/> is called when an item with the same <see cref="ITKDataRepeaterStreamItem.ItemId"/> already exists, this method is called to handle the conflict.
    /// </summary>
    Task<TKDataRepeaterItemMergeConflictResult> HandleAddedDuplicateItemAsync(ITKDataRepeaterStreamItem existingItem, ITKDataRepeaterStreamItem newItem);
}
