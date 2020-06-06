using HealthCheck.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.EventNotifications.Abstractions
{
    /// <summary>
    /// Provides <see cref="EventSinkNotificationConfig"/>s.
    /// </summary>
    public interface IEventSinkNotificationConfigStorage
    {
        /// <summary>
        /// Get all configs for filtering.
        /// </summary>
        IEnumerable<EventSinkNotificationConfig> GetConfigs();

        /// <summary>
        /// Save changes to the given config.
        /// </summary>
        /// <param name="config"></param>
        EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config);

        /// <summary>
        /// Deletes the config with the given id.
        /// </summary>
        void DeleteConfig(Guid configId);
    }
}
