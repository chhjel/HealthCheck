using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Abstractions
{
    /// <summary>
    /// Storage for stream items.
    /// </summary>
    public interface IHCDataRepeaterStreamItemStorage
    {
        /// <summary>
        /// Store a new item.
        /// </summary>
        Task StoreItemAsync(IHCDataRepeaterStreamItem item, object hint = null);

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        Task UpdateItemAsync(IHCDataRepeaterStreamItem item);

        /// <summary>
        /// Get item matching the given id.
        /// </summary>
        Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id);

        /// <summary>
        /// Get item matching the given <see cref="IHCDataRepeaterStreamItem.ItemId"/>.
        /// </summary>
        Task<IHCDataRepeaterStreamItem> GetItemByItemIdAsync(string itemId);

        /// <summary>
        /// Delete item matching the given id.
        /// </summary>
        Task DeleteItemAsync(Guid id);

        /// <summary>
        /// Apply tag to item matching the given id.
        /// </summary>
        Task AddItemTagAsync(Guid id, string tag);

        /// <summary>
        /// Remove tag from item matching the given id.
        /// </summary>
        Task RemoveItemTagAsync(Guid id, string tag);

        /// <summary>
        /// Remove all tags from item matching the given id.
        /// </summary>
        Task RemoveAllItemTagsAsync(Guid id);

        /// <summary>
        /// Allow item matching the given id to be retried processed from the interface.
        /// </summary>
        Task SetAllowItemRetryAsync(Guid id, bool allow);

        /// <summary>
        /// Set a time when the item should be deleted after.
        /// <para>Set to null to not expire.</para>
        /// </summary>
        Task SetItemExpirationTimeAsync(Guid id, DateTimeOffset? time);

        /// <summary>
        /// Get stream items with filter and paging.
        /// </summary>
        Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model);
    }
}
