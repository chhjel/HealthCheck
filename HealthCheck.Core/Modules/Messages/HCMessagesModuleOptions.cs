using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Messages
{
    /// <summary>
    /// Options for <see cref="HCMessagesModule"/>.
    /// </summary>
    public class HCMessagesModuleOptions
    {
        /// <summary>
        /// Messages will be retrieved from here.
        /// </summary>
        public IHCMessageStorage MessageStorage { get; set; }

        internal List<HCMessagesInboxMetadata> InboxMetadata { get; } = new List<HCMessagesInboxMetadata>();

        /// <summary>
        /// Optionally give inboxes a name and description.
        /// </summary>
        public HCMessagesModuleOptions DefineInbox(string inboxId, string name, string description = null)
        {
            InboxMetadata.Add(new HCMessagesInboxMetadata
            {
                Id = inboxId,
                Name = name,
                Description = description
            });
            return this;
        }
    }
}
