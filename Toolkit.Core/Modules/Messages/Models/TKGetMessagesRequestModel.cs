namespace QoDL.Toolkit.Core.Modules.Messages.Models;

/// <summary>
/// Request model sent to messages module.
/// </summary>
public class TKGetMessagesRequestModel
{
    /// <summary>
    /// Inbox to get messages from.
    /// </summary>
    public string InboxId { get; set; }

    /// <summary>
    /// Max number of messages to retrieve.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Index of the page to retrieve.
    /// </summary>
    public int PageIndex { get; set; }
}
