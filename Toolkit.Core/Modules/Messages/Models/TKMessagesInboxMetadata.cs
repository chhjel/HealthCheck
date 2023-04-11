namespace QoDL.Toolkit.Core.Modules.Messages.Models;

/// <summary>
/// Metadata describing an inbox used in the messages module.
/// </summary>
public class TKMessagesInboxMetadata
{
    /// <summary>
    /// Unique id of the inbox.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Name of the inbox to show in the UI.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the inbox to show in the UI.
    /// </summary>
    public string Description { get; set; }
}
