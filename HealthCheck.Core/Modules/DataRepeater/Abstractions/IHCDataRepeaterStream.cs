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
        /// Name of the stream.
        /// </summary>
        string StreamDisplayName { get; }

        /// <summary>
        /// Name of the stream group if any.
        /// </summary>
        string StreamGroupName { get; }

        /// <summary>
        /// Name of the item id.
        /// </summary>
        string ItemIdDisplayName { get; }

        /// <summary>
        /// List of initially selected tags in the UI for this stream.
        /// </summary>
        List<string> InitiallySelectedTags { get; }

        /// <summary>
        /// List of tags that can be applied manually in the UI.
        /// </summary>
        List<string> ManuallyAllowedTags { get; }

        /// <summary>
        /// Store a new item.
        /// </summary>
        Task StoreItemAsync(IHCDataRepeaterStreamItem item, object hint = null);

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        Task UpdateItemAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Get item matching either id or itemId.
        /// </summary>
        Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id, string itemId);

        /// <summary>
        /// Delete item matching either id or itemId.
        /// </summary>
        Task DeleteItemAsync(Guid id, string itemId);

        /// <summary>
        /// Apply tag to item matching either id or itemId.
        /// </summary>
        Task AddItemTagAsync(Guid id, string itemId, string tag);

        /// <summary>
        /// Remove tag from item matching either id or itemId.
        /// </summary>
        Task RemoveItemTagAsync(Guid id, string itemId, string tag);

        /// <summary>
        /// Remove all tags from item matching either id or itemId.
        /// </summary>
        Task RemoveAllItemTagsAsync(Guid id, string itemId);

        /// <summary>
        /// Allow item matching either id or itemId to be retried processed from the interface.
        /// </summary>
        Task SetAllowItemRetryAsync(Guid id, string itemId, bool allow);

        /// <summary>
        /// Retry the given item.
        /// </summary>
        Task<HCDataRepeaterActionResult> RetryItemAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Analyze item for potential issues and apply suitable tags.
        /// </summary>
        Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Get stream items with filter and paging.
        /// </summary>
        Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model);

        /// <summary>
        /// Extra details about an item to display in the UI.
        /// </summary>
        Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id, string itemId);
    }
}
