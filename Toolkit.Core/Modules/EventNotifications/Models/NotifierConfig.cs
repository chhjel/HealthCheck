using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Models
{
    /// <summary>
    /// Option values for this notifier instance.
    /// </summary>
    public class NotifierConfig
    {
        /// <summary>
        /// Id of notifier to notify when events match.
        /// </summary>
        public string NotifierId { get; set; }

        /// <summary>
        /// Option values for this notifier instance.
        /// </summary>
        public Dictionary<string, string> Options { get; set; }
    }
}
