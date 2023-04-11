using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Messages;

/// <summary>
/// Options for <see cref="TKMessagesModule"/>.
/// </summary>
public class TKMessagesModuleOptions
{
    /// <summary>
    /// Messages will be retrieved from here.
    /// </summary>
    public ITKMessageStorage MessageStorage { get; set; }

    internal List<TKMessagesInboxMetadata> InboxMetadata { get; } = new List<TKMessagesInboxMetadata>();

    /// <summary>
    /// Optionally give inboxes a name and description.
    /// </summary>
    public TKMessagesModuleOptions DefineInbox(string inboxId, string name, string description = null)
    {
        InboxMetadata.Add(new TKMessagesInboxMetadata
        {
            Id = inboxId,
            Name = name,
            Description = description
        });
        return this;
    }
}
