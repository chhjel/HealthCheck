using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Utils
{
    /// <summary>
    /// Utilities related to the data repeater module.
    /// <para>Any exceptions ignored by the methods can be logged by subscribing to <see cref="TKGlobalConfig.OnExceptionEvent"/>.</para>
    /// </summary>
    public static class TKDataRepeaterUtils
    {
        /// <summary>
        /// Default max number of log entries to store per item when using utility and extension methods.
        /// <para>Defaults to 20.</para>
        /// </summary>
        public static int DefaultMaxItemLogEntries { get; set; } = 20;

        /// <summary>
        /// Adds a new item to the first registered stream of the given type.
        /// <para>If an existing item with the same item id is found, <see cref="ITKDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void AddStreamItem<TStream>(ITKDataRepeaterStreamItem item, object hint = null, bool analyze = true)
            => Task.Run(() => AddStreamItemAsync<TStream>(item, hint, analyze));

        /// <summary>
        /// Adds a new item to the first registered stream of the given type.
        /// <para>If an existing item with the same item id is found, <see cref="ITKDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task AddStreamItemAsync<TStream>(ITKDataRepeaterStreamItem item, object hint = null, bool analyze = true, bool handleDuplicates = true)
        {
            try
            {
                if (TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(ITKDataRepeaterService)) is not ITKDataRepeaterService service) return;
                await service.AddStreamItemAsync<TStream>(item, hint, analyze, handleDuplicates);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(AddStreamItemAsync), ex);
            }
        }

        /// <summary>
        /// Adds a batch of new items to the first registered stream of the given type.
        /// <para>If an existing item with the same item id is found, <see cref="ITKDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void AddStreamItems<TStream>(IEnumerable<ITKDataRepeaterStreamItem> items, object hint = null, bool analyze = true)
            => Task.Run(() => AddStreamItemsAsync<TStream>(items, hint, analyze));

        /// <summary>
        /// Adds a batch of new items to the first registered stream of the given type.
        /// <para>If an existing item with the same item id is found, <see cref="ITKDataRepeaterStream.HandleAddedDuplicateItemAsync"/> will be called.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task AddStreamItemsAsync<TStream>(IEnumerable<ITKDataRepeaterStreamItem> items, object hint = null, bool analyze = true, bool handleDuplicates = true)
        {
            try
            {
                if (TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(ITKDataRepeaterService)) is not ITKDataRepeaterService service) return;
                await service.AddStreamItemsAsync<TStream>(items, hint, analyze, handleDuplicates);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(AddStreamItemsAsync), ex);
            }
        }

        /// <summary>
        /// Gets the first registered stream of the given type.
        /// <para>If not found returns null.</para>
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static ITKDataRepeaterStream GetStream<TStream>()
        {
            try
            {
                var service = TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(ITKDataRepeaterService)) as ITKDataRepeaterService;
                var streams = service?.GetStreams();
                return streams?.FirstOrDefault(x => x.GetType() == typeof(TStream));
            }
            catch(Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(GetStream), ex);
                return null;
            }
        }

        /// <summary>
        /// Get item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static ITKDataRepeaterStreamItem GetItemByItemId<TStream>(string itemId)
            => TKAsyncUtils.RunSync(() => GetItemByItemIdAsync<TStream>(itemId));

        /// <summary>
        /// Get item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<ITKDataRepeaterStreamItem> GetItemByItemIdAsync<TStream>(string itemId)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return null;
                return await stream.GetItemByItemIdAsync(itemId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(GetItemByItemIdAsync), ex);
                return null;
            }
        }

        /// <summary>
        /// Delete item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool DeleteItem<TStream>(string itemId)
            => TKAsyncUtils.RunSync(() => DeleteItemAsync<TStream>(itemId));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(DeleteItemAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Modifies an existing item from the given stream with the given id.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static ITKDataRepeaterStreamItem ModifyItem<TStream>(string itemId, Action<ITKDataRepeaterStreamItem> modification)
            => TKAsyncUtils.RunSync(() => ModifyItemAsync<TStream>(itemId, modification));

        /// <summary>
        /// Modifies an existing item from the given stream with the given id.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<ITKDataRepeaterStreamItem> ModifyItemAsync<TStream>(string itemId, Action<ITKDataRepeaterStreamItem> modification)
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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(ModifyItemAsync), ex);
                return null;
            }
        }

        /// <summary>
        /// Modifies existing items from the given stream with the given ids.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<List<ITKDataRepeaterStreamItem>> ModifyItemsAsync<TStream>(IEnumerable<string> itemIds, Action<ITKDataRepeaterStreamItem> modification)
        {
            try
            {
                if (itemIds?.Any() != true) return new List<ITKDataRepeaterStreamItem>();

                var stream = GetStream<TStream>();
                if (stream == null) return null;

                var itemIdsHashSet = new HashSet<string>(itemIds);
                var items = (await stream.Storage.GetAllItemsAsync().ConfigureAwait(false))
                    ?.Where(x => itemIdsHashSet.Contains(x.ItemId))
                    ?.ToList();
                if (items.Count > 0)
                {
                    var batch = new TKDataRepeaterBatchedStorageItemActions();
                    foreach (var item in items)
                    {
                        modification?.Invoke(item);
                        batch.Updates.Add(new TKDataRepeaterBatchedStorageItemAction(item, null));
                    }
                    await stream.Storage.PerformBatchUpdateAsync(batch);
                }
                return items;
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(ModifyItemsAsync), ex);
                return null;
            }
        }

        /// <summary>
        /// Remove all tags from the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool RemoveAllItemTags<TStream>(string itemId)
            => TKAsyncUtils.RunSync(() => RemoveAllItemTagsAsync<TStream>(itemId));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(RemoveAllItemTagsAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Remove a single tag from the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool RemoveItemTag<TStream>(string itemId, string tag)
            => TKAsyncUtils.RunSync(() => RemoveItemTagAsync<TStream>(itemId, tag));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(RemoveItemTagAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Remove tags from item matching the given id.
        /// </summary>
        public static bool RemoveItemTags<TStream>(string itemId, params string[] tags)
            => TKAsyncUtils.RunSync(() => RemoveItemTagsAsync<TStream>(itemId, tags));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(RemoveItemTagsAsync), ex);
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
            => TKAsyncUtils.RunSync(() => SetTagsAsync<TStream>(itemId, tags, removeOtherTags));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(SetTagsAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Add a tag to the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool AddItemTag<TStream>(string itemId, string tag)
            => TKAsyncUtils.RunSync(() => AddItemTagAsync<TStream>(itemId, tag));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(AddItemTagAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Add tags to the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool AddItemTags<TStream>(string itemId, params string[] tags)
            => TKAsyncUtils.RunSync(() => AddItemTagsAsync<TStream>(itemId, tags));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(AddItemTagsAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Set if the item matching the given item id from the stream of the given type is allowed to be retried.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool SetAllowItemRetry<TStream>(string itemId, bool allow)
            => TKAsyncUtils.RunSync(() => SetAllowItemRetryAsync<TStream>(itemId, allow));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(SetAllowItemRetryAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Toggle allow retry on the item matching the given item id from the stream of the given type.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static bool SetExpirationTime<TStream>(string itemId, DateTimeOffset? time)
            => TKAsyncUtils.RunSync(() => SetExpirationTimeAsync<TStream>(itemId, time));

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
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(SetExpirationTimeAsync), ex);
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
        /// <param name="error">Optional error.</param>
        /// <param name="exception">Optional exception.</param>
        public static bool SetForcedItemStatus<TStream>(string itemId, TKDataRepeaterStreamItemStatus? status,
            Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null,
            string error = null, Exception exception = null)
            => TKAsyncUtils.RunSync(() => SetForcedItemStatusAsync<TStream>(itemId, status, expirationTime, logMessage, error, exception));

        /// <summary>
        /// Set the given items forced status that is only used to override status colors in the UI by default.
        /// <para>Optionally provide a log message and an expiration time.</para>
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="status">Status to enforce. Can be null to clear forced status.</param>
        /// <param name="expirationTime">Optionally set expiration time. Null = no effect, Maybe{null} = clear.</param>
        /// <param name="logMessage">Optional log message.</param>
        /// <param name="error">Optional error.</param>
        /// <param name="exception">Optional exception.</param>
        public static async Task<bool> SetForcedItemStatusAsync<TStream>(string itemId, TKDataRepeaterStreamItemStatus? status,
            Maybe<DateTimeOffset?> expirationTime = null, string logMessage = null,
            string error = null, Exception exception = null)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.SetForcedItemStatusAsync(itemId, status, expirationTime, logMessage, error, exception).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(SetForcedItemStatusAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Adds a log message to the given item.
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="logMessage">Message to log.</param>
        public static bool AddItemLogMessage<TStream>(string itemId, string logMessage)
            => TKAsyncUtils.RunSync(() => AddItemLogMessageAsync<TStream>(itemId, logMessage));

        /// <summary>
        /// Adds a log message to the given item.
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="logMessage">Message to log.</param>
        public static async Task<bool> AddItemLogMessageAsync<TStream>(string itemId, string logMessage)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.AddItemLogMessageAsync(itemId, logMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(AddItemLogMessageAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Sets Error to the given message and optionally include exception details.
        /// <para>If FirstError is empty it will be updated as well.</para>
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="error">Error to set.</param>
        /// <param name="exception">Exception to include details from if any.</param>
        public static bool SetItemError<TStream>(string itemId, string error, Exception exception = null)
            => TKAsyncUtils.RunSync(() => SetItemErrorAsync<TStream>(itemId, error, exception));

        /// <summary>
        /// Sets Error to the given message and optionally include exception details.
        /// <para>If FirstError is empty it will be updated as well.</para>
        /// </summary>
        /// <param name="itemId">Id of item to target.</param>
        /// <param name="error">Error to set.</param>
        /// <param name="exception">Exception to include details from if any.</param>
        public static async Task<bool> SetItemErrorAsync<TStream>(string itemId, string error, Exception exception = null)
        {
            try
            {
                var stream = GetStream<TStream>();
                if (stream == null) return false;
                return await stream.SetItemErrorAsync<TStream>(itemId, error, exception).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKDataRepeaterUtils), nameof(AddItemLogMessageAsync), ex);
                return false;
            }
        }

        /// <summary>
        /// Applies the given changes to the given item.
        /// </summary>
        internal static void ApplyChangesToItem(ITKDataRepeaterStreamItem item, TKDataItemChangeBase changes)
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

            if (changes?.StreamItemModification != null)
            {
                changes.StreamItemModification.Invoke(item);
            }
        }
    }
}
