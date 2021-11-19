using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IHCDataRepeaterStream"/>
    /// </summary>
    public static class HHCDataRepeaterStreamExtensions
    {
        /// <summary>
        /// Get item matching the given id.
        /// </summary>
        public static async Task<IHCDataRepeaterStreamItem> GetItemAsync(this IHCDataRepeaterStream stream, Guid id)
            => await stream.GetItemAsync(id, null).ConfigureAwait(false);

        /// <summary>
        /// Delete item matching the given id.
        /// </summary>
        public static async Task DeleteItemAsync(this IHCDataRepeaterStream stream, Guid id)
            => await stream.DeleteItemAsync(id, null).ConfigureAwait(false);

        /// <summary>
        /// Remove all tags from the item matching the given id.
        /// </summary>
        public static async Task RemoveAllItemTagsAsync(this IHCDataRepeaterStream stream, Guid id)
            => await stream.RemoveAllItemTagsAsync(id, null).ConfigureAwait(false);

        /// <summary>
        /// Remove a single tag from the item matching the given id.
        /// </summary>
        public static async Task RemoveItemTagAsync(this IHCDataRepeaterStream stream, Guid id, string tag)
            => await stream.RemoveItemTagAsync(id, null, tag).ConfigureAwait(false);

        /// <summary>
        /// Add a tag to the item matching the given id.
        /// </summary>
        public static async Task AddItemTagAsync(this IHCDataRepeaterStream stream, Guid id, string tag)
            => await stream.AddItemTagAsync(id, null, tag).ConfigureAwait(false);

        /// <summary>
        /// Toggle allow retry on the item matching the given id.
        /// </summary>
        public static async Task SetAllowItemRetryAsync(this IHCDataRepeaterStream stream, Guid id, bool allow)
            => await stream.SetAllowItemRetryAsync(id, null, allow).ConfigureAwait(false);
    }
}
