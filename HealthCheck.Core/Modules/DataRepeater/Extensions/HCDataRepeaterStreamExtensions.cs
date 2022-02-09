using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Add tags to the item matching the given item id.
        /// </summary>
        public static async Task<bool> AddItemTagsAsync(this IHCDataRepeaterStream stream, string itemId, params string[] tags)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.AddItemTagsAsync(item.Id, tags).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Remove tags from the item matching the given item id.
        /// </summary>
        public static async Task<bool> RemoveItemTagsAsync(this IHCDataRepeaterStream stream, string itemId, params string[] tags)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.RemoveItemTagsAsync(item.Id, tags).ConfigureAwait(false);
            return true;
        }
        
        /// <summary>
        /// Add/remove the given tags on the item matching the given item id, and optionally remove all others.
        /// </summary>
        /// <param name="stream">Target stream</param>
        /// <param name="itemId">Target item</param>
        /// <param name="tags">True to add a tag, false to remove a tag.</param>
        /// <param name="removeOtherTags">True to remove all other tags if any.</param>
        public static async Task<bool> SetTagsAsync(this IHCDataRepeaterStream stream, string itemId, Dictionary<string, bool> tags, bool removeOtherTags = false)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;

            if (!removeOtherTags)
            {
                var toRemove = tags.Where(x => x.Value == false).Select(x => x.Key).ToArray();
                if (toRemove.Any())
                {
                    await stream.Storage.RemoveItemTagsAsync(item.Id, toRemove).ConfigureAwait(false);
                }
            }
            else if (item.Tags?.Any() == true)
            {
                await stream.Storage.RemoveAllItemTagsAsync(item.Id).ConfigureAwait(false);
            }

            var toAdd = tags.Where(x => x.Value == true).Select(x => x.Key).ToArray();
            if (toAdd.Any())
            {
                await stream.Storage.AddItemTagsAsync(item.Id, toAdd).ConfigureAwait(false);
            }

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

        /// <summary>
        /// Set the given items forced status that is only used to override status colors in the UI by default.
        /// <para>Optionally provide a log message and an expiration time.</para>
        /// </summary>
        /// <param name="stream">Stream to target.</param>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="status">Status to enforce. Can be null to clear forced status.</param>
        /// <param name="expirationTime">Optionally set expiration time. Null = no effect, Maybe{null} = clear.</param>
        /// <param name="logMessage">Optional log message.</param>
        /// <param name="error">Optional error.</param>
        /// <param name="exception">Optional exception.</param>
        public static async Task<bool> SetForcedItemStatusAsync(this IHCDataRepeaterStream stream, string itemId, HCDataRepeaterStreamItemStatus? status,
            Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null,
            string error = null, Exception exception = null)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;

            error ??= "";
            if (exception != null)
            {
                error += $"\n\n{HCExceptionUtils.GetFullExceptionDetails(exception)}";
            }

            await stream.Storage.SetForcedItemStatusAsync(item.Id, status, expirationTime, logMessage, error).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Adds a log message to the given item.
        /// </summary>
        /// <param name="stream">Stream to target.</param>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="logMessage">Message to log.</param>
        public static async Task<bool> AddItemLogMessageAsync(this IHCDataRepeaterStream stream, string itemId, string logMessage)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;
            await stream.Storage.AddItemLogMessageAsync(item.Id, logMessage).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Sets Error to the given message and optionally include exception details.
        /// <para>If FirstError is empty it will be updated as well.</para>
        /// </summary>
        /// <param name="stream">Stream to target.</param>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="error">Error to set.</param>
        /// <param name="exception">Exception to include details from if any.</param>
        public static async Task<bool> SetItemErrorAsync<TStream>(this IHCDataRepeaterStream stream, string itemId, string error, Exception exception = null)
        {
            var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            if (item == null) return false;

            error ??= "";
            if (exception != null)
            {
                error += $"\n\n{HCExceptionUtils.GetFullExceptionDetails(exception)}";
            }

            await stream.Storage.SetItemErrorAsync(item.Id, error).ConfigureAwait(false);
            return true;
        }
    }
}
