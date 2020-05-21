using HealthCheck.Core.Modules.EventNotifications.Abstractions;

namespace HealthCheck.Core.Modules.EventNotifications
{
    /// <summary>
    /// Options for <see cref="HCEventNotificationsModule"/>.
    /// </summary>
    public class HCEventNotificationsModuleOptions
    {
        /// <summary>
        /// Handles events.
        /// </summary>
        public IEventDataSink EventSink { get; set; }
    }
}
