namespace HealthCheck.Core.Modules.Messages.Models
{
    /// <summary>
    /// Request model sent to messages module.
    /// </summary>
    public class HCDeleteMessageRequestModel
    {
        /// <summary>
        /// Inbox to delete from.
        /// </summary>
        public string InboxId { get; set; }

        /// <summary>
        /// Message id to delete.
        /// </summary>
        public string MessageId { get; set; }
    }
}
