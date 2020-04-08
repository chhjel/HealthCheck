using HealthCheck.Core.Modules.EventNotifications;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Model used when retrieving event notification configs.
    /// </summary>
    public class GetEventNotificationConfigsViewModel
    {
        /// <summary>
        /// Available notifiers.
        /// </summary>
        public IEnumerable<IEventNotifier> Notifiers { get; set; } = Enumerable.Empty<IEventNotifier>();

        /// <summary>
        /// All defined configs.
        /// </summary>
        public IEnumerable<EventSinkNotificationConfig> Configs { get; set; }
        
        /// <summary>
        /// All known event definitions.
        /// </summary>
        public IEnumerable<KnownEventDefinition> KnownEventDefinitions { get; set; }

        /// <summary>
        /// All placeholders that should work for all notifier options.
        /// </summary>
        public IEnumerable<string> Placeholders { get; set; }
    }
}
