using HealthCheck.Core.Modules.Messages.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Messages.Abstractions
{
    /// <summary>
    /// Stores messages for use in the messages module.
    /// </summary>
    public interface IHCMessageStorage
    {
        /// <summary>
        /// Add a new message to the given inbox.
        /// </summary>
        Task StoreMessageAsync(string inboxId, IHCMessageItem message);

        /// <summary>
        /// Get the latest messages from the given inbox.
        /// </summary>
        Task<HCDataWithTotalCount<IEnumerable<IHCMessageItem>>> GetLatestMessagesAsync(string inboxId, int pageSize, int pageIndex);

        /// <summary>
        /// Get a single messages from the given inbox.
        /// </summary>
        Task<IHCMessageItem> GetMessageAsync(string inboxId, string messageId);

        /// <summary>
        /// Delete a message with the given id.
        /// </summary>
        Task<bool> DeleteMessageAsync(string inboxId, string messageId);

        /// <summary>
        /// Delete a whole inbox with the given id.
        /// </summary>
        Task<bool> DeleteInboxAsync(string inboxId);

        /// <summary>
        /// Delete all stored data.
        /// </summary>
        Task DeleteAllDataAsync();
    }
}
