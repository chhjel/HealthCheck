using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;

namespace QoDL.Toolkit.Core.Modules.EventNotifications
{
    /// <summary>
    /// Options for <see cref="TKEventNotificationsModule"/>.
    /// </summary>
    public class TKEventNotificationsModuleOptions
    {
        /// <summary>
        /// Handles events.
        /// </summary>
        public IEventDataSink EventSink { get; set; }
    }
}
