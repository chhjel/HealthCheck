using System.Collections.Generic;

namespace HealthCheck.Core.Modules.EventNotifications
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

        /// <summary>
        /// Get the option with the given id/key, or null if not present.
        /// </summary>
        public string GetOption(string id)
            => (id != null && Options.ContainsKey(id)) ? Options[id] : null;
    }
}
