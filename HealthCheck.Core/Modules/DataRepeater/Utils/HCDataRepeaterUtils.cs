using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Extensions;
using HealthCheck.Core.Modules.DataRepeater.Models;
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
        /// Gets the first registered stream of the given type.
        /// <para>If not found returns null.</para>
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
        /// Toggle allow retry on the item matching the given item id from the stream of the given type.
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
