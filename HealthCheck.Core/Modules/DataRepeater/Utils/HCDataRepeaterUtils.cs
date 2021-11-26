using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Extensions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Utils
{
    /// <summary>
    /// Utilities related to the data repeater module.
    /// </summary>
    public static class HCDataRepeaterUtils
    {
        /// <summary>
        /// Adds a new item to the first registered stream of the given type.
        /// <para>If an existing item with the same item id is found, <see cref="IHCDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void AddStreamItem<TStream>(IHCDataRepeaterStreamItem item, object hint = null, bool analyze = true)
            => Task.Run(() => AddStreamItemAsync<TStream>(item, hint, analyze));

        /// <summary>
        /// Adds a new item to the first registered stream of the given type.
        /// <para>If an existing item with the same item id is found, <see cref="IHCDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task AddStreamItemAsync<TStream>(IHCDataRepeaterStreamItem item, object hint = null, bool analyze = true, bool handleDuplicates = true)
        {
            try
            {
                if (HCGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHCDataRepeaterService)) is not IHCDataRepeaterService service) return;
                await service.AddStreamItemAsync<TStream>(item, hint, analyze, handleDuplicates);
            }
            catch (Exception)
            {
                /* Ignored */
            }
        }

        /// <summary>
        /// Gets the first registered stream of the given type.
        /// <para>If not found returns null.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static IHCDataRepeaterStream GetStream<TStream>()
        {
            try
            {
                var service = HCGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHCDataRepeaterService)) as IHCDataRepeaterService;
                var streams = service?.GetStreams();
                return streams?.FirstOrDefault(x => x.GetType() == typeof(TStream));
            }
            catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static IHCDataRepeaterStreamItem GetItemByItemId<TStream>(string itemId)
            => AsyncUtils.RunSync(() => GetItemByItemIdAsync<TStream>(itemId));

        /// <summary>
        /// Get item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<IHCDataRepeaterStreamItem> GetItemByItemIdAsync<TStream>(string itemId)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return null;
                return await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Delete item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool DeleteItem<TStream>(string itemId)
            => AsyncUtils.RunSync(() => DeleteItemAsync<TStream>(itemId));

        /// <summary>
        /// Delete item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> DeleteItemAsync<TStream>(string itemId)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.DeleteItemAsync(itemId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Modifies an existing item from the given stream with the given id.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static IHCDataRepeaterStreamItem ModifyItem<TStream>(string itemId, Action<IHCDataRepeaterStreamItem> modification)
            => AsyncUtils.RunSync(() => ModifyItemAsync<TStream>(itemId, modification));

        /// <summary>
        /// Modifies an existing item from the given stream with the given id.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<IHCDataRepeaterStreamItem> ModifyItemAsync<TStream>(string itemId, Action<IHCDataRepeaterStreamItem> modification)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return null;
                var item = await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
                if (item != null)
                {
                    modification?.Invoke(item);
                    await stream.Storage.UpdateItemAsync(item);
                }
                return item;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Remove all tags from the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool RemoveAllItemTags<TStream>(string itemId)
            => AsyncUtils.RunSync(() => RemoveAllItemTagsAsync<TStream>(itemId));

        /// <summary>
        /// Remove all tags from the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> RemoveAllItemTagsAsync<TStream>(string itemId)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.RemoveAllItemTagsAsync(itemId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove a single tag from the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool RemoveItemTag<TStream>(string itemId, string tag)
            => AsyncUtils.RunSync(() => RemoveItemTagAsync<TStream>(itemId, tag));

        /// <summary>
        /// Remove a single tag from the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> RemoveItemTagAsync<TStream>(string itemId, string tag)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.RemoveItemTagAsync(itemId, tag).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove tags from item matching the given id.
        /// </summary>
        public static bool RemoveItemTags<TStream>(string itemId, params string[] tags)
            => AsyncUtils.RunSync(() => RemoveItemTagsAsync<TStream>(itemId, tags));

        /// <summary>
        /// Remove tags from item matching the given id.
        /// </summary>
        public static async Task<bool> RemoveItemTagsAsync<TStream>(string itemId, params string[] tags)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.RemoveItemTagsAsync(itemId, tags).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add/remove the given tags on the item matching the given item id, and optionally remove all others.
        /// </summary>
        /// <param name="itemId">Target item</param>
        /// <param name="tags">True to add a tag, false to remove a tag.</param>
        /// <param name="removeOtherTags">True to remove all other tags if any.</param>
        public static bool SetTags<TStream>(string itemId, Dictionary<string, bool> tags, bool removeOtherTags = false)
            => AsyncUtils.RunSync(() => SetTagsAsync<TStream>(itemId, tags, removeOtherTags));

        /// <summary>
        /// Add/remove the given tags on the item matching the given item id, and optionally remove all others.
        /// </summary>
        /// <param name="itemId">Target item</param>
        /// <param name="tags">True to add a tag, false to remove a tag.</param>
        /// <param name="removeOtherTags">True to remove all other tags if any.</param>
        public static async Task<bool> SetTagsAsync<TStream>(string itemId, Dictionary<string, bool> tags, bool removeOtherTags = false)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.SetTagsAsync(itemId, tags, removeOtherTags).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add a tag to the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool AddItemTag<TStream>(string itemId, string tag)
            => AsyncUtils.RunSync(() => AddItemTagAsync<TStream>(itemId, tag));

        /// <summary>
        /// Add a tag to the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> AddItemTagAsync<TStream>(string itemId, string tag)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.AddItemTagAsync(itemId, tag).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add tags to the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool AddItemTags<TStream>(string itemId, params string[] tags)
            => AsyncUtils.RunSync(() => AddItemTagsAsync<TStream>(itemId, tags));

        /// <summary>
        /// Add tags to the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> AddItemTagsAsync<TStream>(string itemId, params string[] tags)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.AddItemTagsAsync(itemId, tags).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Set if the item matching the given item id from the stream of the given type is allowed to be retried.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool SetAllowItemRetry<TStream>(string itemId, bool allow)
            => AsyncUtils.RunSync(() => SetAllowItemRetryAsync<TStream>(itemId, allow));

        /// <summary>
        /// Set if the item matching the given item id from the stream of the given type is allowed to be retried.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> SetAllowItemRetryAsync<TStream>(string itemId, bool allow)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.SetAllowItemRetryAsync(itemId, allow).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Toggle allow retry on the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool SetExpirationTime<TStream>(string itemId, DateTimeOffset? time)
            => AsyncUtils.RunSync(() => SetExpirationTimeAsync<TStream>(itemId, time));

        /// <summary>
        /// Toggle allow retry on the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<bool> SetExpirationTimeAsync<TStream>(string itemId, DateTimeOffset? time)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.SetExpirationTimeAsync(itemId, time).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Set the given items forced status that is only used to override status colors in the UI by default.
        /// <para>Optionally provide a log message and an expiration time.</para>
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="status">Status to enforce. Can be null to clear forced status.</param>
        /// <param name="expirationTime">Optionally set expiration time. Null = no effect, Maybe{null} = clear.</param>
        /// <param name="logMessage">Optional log message.</param>
        public static bool SetForcedItemStatus<TStream>(string itemId, HCDataRepeaterStreamItemStatus? status, Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null)
            => AsyncUtils.RunSync(() => SetForcedItemStatusAsync<TStream>(itemId, status, expirationTime, logMessage));

        /// <summary>
        /// Set the given items forced status that is only used to override status colors in the UI by default.
        /// <para>Optionally provide a log message and an expiration time.</para>
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="status">Status to enforce. Can be null to clear forced status.</param>
        /// <param name="expirationTime">Optionally set expiration time. Null = no effect, Maybe{null} = clear.</param>
        /// <param name="logMessage">Optional log message.</param>
        public static async Task<bool> SetForcedItemStatusAsync<TStream>(string itemId, HCDataRepeaterStreamItemStatus? status, Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.SetForcedItemStatusAsync(itemId, status, expirationTime, logMessage).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Applies changes to the given item.
        /// </summary>
        public static void ApplyChangesToItem(IHCDataRepeaterStreamItem item, HCDataItemChangeBase changes)
        {
            if (changes == null || item == null) return;

            if (changes?.AllowRetry != null)
            {
                item.AllowRetry = changes.AllowRetry.Value;
            }

            item.Tags ??= new HashSet<string>();
            if (changes.RemoveAllTags && item.Tags?.Any() == true)
            {
                item.Tags.Clear();
            }

            if (changes?.TagsThatShouldNotExist?.Any() == true)
            {
                foreach (var tag in changes.TagsThatShouldNotExist)
                {
                    item.Tags.Remove(tag);
                }
            }

            if (changes?.TagsThatShouldExist?.Any() == true)
            {
                foreach (var tag in changes.TagsThatShouldExist)
                {
                    item.Tags.Add(tag);
                }
            }
        }
    }
}
