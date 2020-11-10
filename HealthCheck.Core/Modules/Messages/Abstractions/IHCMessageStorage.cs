using HealthCheck.Core.Modules.Messages.Models;
using System.Collections.Generic;

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
        void StoreMessage(string inboxId, IHCMessageItem message);

        /// <summary>
        /// Get the latest messages from the given inbox.
        /// </summary>
        HCDataWithTotalCount<IEnumerable<IHCMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex);

        /// <summary>
        /// Get a single messages from the given inbox.
        /// </summary>
        IHCMessageItem GetMessage(string inboxId, string messageId);

        /// <summary>
        /// Delete a message with the given id.
        /// </summary>
        bool DeleteMessage(string inboxId, string messageId);

        /// <summary>
        /// Delete a whole inbox with the given id.
        /// </summary>
        bool DeleteInbox(string inboxId);

        /// <summary>
        /// Delete all stored data.
        /// </summary>
        void DeleteAllData();
    }
}
