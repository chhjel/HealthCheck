using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IHCDataRepeaterStream"/>
    /// </summary>
    public static class HCDataRepeaterStreamExtensions
    {
        /// <summary>
        /// Get item matching the given item id.
        /// </summary>
        public static async Task<IHCDataRepeaterStreamItem> GetItemByItemIdAsync(this IHCDataRepeaterStream stream, string itemId)
            => await stream.Storage.GetItemByItemIdAsync(itemId).ConfigureAwait(false);

        /// <summary>
        /// Delete item matching the given item id.
        /// </summary>
        public static async Task<bool> DeleteItemAsync(this IHCDataRepeaterStream stream, string itemId)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.DeleteItemAsync(item.Id).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Remove all tags from the item matching the given item id.
        /// </summary>
        public static async Task<bool> RemoveAllItemTagsAsync(this IHCDataRepeaterStream stream, string itemId)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.RemoveAllItemTagsAsync(item.Id).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Remove a single tag from the item matching the given item id.
        /// </summary>
        public static async Task<bool> RemoveItemTagAsync(this IHCDataRepeaterStream stream, string itemId, string tag)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.RemoveItemTagAsync(item.Id, tag).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Add a tag to the item matching the given item id.
        /// </summary>
        public static async Task<bool> AddItemTagAsync(this IHCDataRepeaterStream stream, string itemId, string tag)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.AddItemTagAsync(item.Id, tag).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Add a tag to the item matching the given item id.
        /// </summary>
        public static async Task<bool> AddItemTagsAsync(this IHCDataRepeaterStream stream, string itemId, params string[] tags)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.AddItemTagsAsync(item.Id, tags).ConfigureAwait(false);
            return true;
        }
        
        /// <summary>
        /// Toggle allow retry on the item matching the given item id.
        /// </summary>
        public static async Task<bool> SetAllowItemRetryAsync(this IHCDataRepeaterStream stream, string itemId, bool allow)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.SetAllowItemRetryAsync(item.Id, allow).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Sets optional expiration time on the given item.
        /// </summary>
        public static async Task<bool> SetExpirationTimeAsync(this IHCDataRepeaterStream stream, string itemId, DateTimeOffset? time)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.SetItemExpirationTimeAsync(item.Id, time).ConfigureAwait(false);
            return true;
        }
    }
}
