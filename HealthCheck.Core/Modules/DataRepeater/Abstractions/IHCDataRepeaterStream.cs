﻿using HealthCheck.Core.Modules.DataRepeater.Models;
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
        /// <para>Must be a flags enum of the same type as the one used on the healthcheck controller.</para>
        /// </summary>
        object AllowedAccessRoles { get; }

        /// <summary>
        /// Optional categories this stream belongs to.
        /// <para>Can be used for more granular access configuration.</para>
        /// </summary>
        List<string> Categories { get; }

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
        Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// If <see cref="IHCDataRepeaterStreamItemStorage.AddItemAsync"/> is called when an item with the same <see cref="IHCDataRepeaterStreamItem.ItemId"/> already exists, this method is called to handle the conflict.
        /// </summary>
        Task<HCDataRepeaterItemMergeConflictResult> HandleAddedDuplicateItemAsync(IHCDataRepeaterStreamItem existingItem, IHCDataRepeaterStreamItem newItem);
    }
}
