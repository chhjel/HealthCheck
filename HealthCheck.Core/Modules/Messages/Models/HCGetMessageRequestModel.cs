namespace HealthCheck.Core.Modules.Messages.Models
{
    /// <summary>
    /// Request model sent to messages module.
    /// </summary>
    public class HCGetMessageRequestModel
    {
        /// <summary>
        /// Inbox to get messages from.
        /// </summary>
        public string InboxId { get; set; }

        /// <summary>
        /// Message id.
        /// </summary>
        public string MessageId { get; set; }
    }
}
