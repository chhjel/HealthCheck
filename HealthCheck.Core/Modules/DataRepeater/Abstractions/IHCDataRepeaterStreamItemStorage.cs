using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
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
        Task AddItemAsync(IHCDataRepeaterStreamItem item, object hint = null);

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
        /// Apply tags to item matching the given id.
        /// </summary>
        Task AddItemTagsAsync(Guid id, params string[] tags);

        /// <summary>
        /// Remove tag from item matching the given id.
        /// </summary>
        Task RemoveItemTagAsync(Guid id, string tag);

        /// <summary>
        /// Remove tags from item matching the given id.
        /// </summary>
        Task RemoveItemTagsAsync(Guid id, params string[] tags);

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
        /// Set the given items forced status that is only used to override status colors in the UI by default.
        /// <para>Optionally provide a log message and an expiration time.</para>
        /// </summary>
        /// <param name="id">Id of item to target.</param>
        /// <param name="status">Status to enforce. Can be null to clear forced status.</param>
        /// <param name="expirationTime">Optionally set expiration time. Null = no effect, Maybe{null} = clear.</param>
        /// <param name="logMessage">Optional log message.</param>
        /// <param name="error">Optional error.</param>
        Task SetForcedItemStatusAsync(Guid id, HCDataRepeaterStreamItemStatus? status,
            Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null, string error = null);

        /// <summary>
        /// Adds a log message to the given item.
        /// </summary>
        /// <param name="id">Id of item to target.</param>
        /// <param name="logMessage">Message to log.</param>
        Task AddItemLogMessageAsync(Guid id, string logMessage);

        /// <summary>
        /// Sets Error to the given message and optionally include exception details.
        /// <para>If FirstError is empty it will be updated as well.</para>
        /// </summary>
        /// <param name="id">Id of item to target.</param>
        /// <param name="error">Error to set.</param>
        Task SetItemErrorAsync(Guid id, string error);

        /// <summary>
        /// Get stream items with filter and paging.
        /// </summary>
        Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model);

        /// <summary>
        /// Get all stored items.
        /// <para>Only used for custom batch actions if any.</para>
        /// </summary>
        Task<IEnumerable<IHCDataRepeaterStreamItem>> GetAllItemsAsync();
    }
}
