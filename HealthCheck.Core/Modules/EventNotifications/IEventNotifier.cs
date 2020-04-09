using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.EventNotifications
{
    /// <summary>
    /// Notifies events.
    /// </summary>
    public interface IEventNotifier
    {
        /// <summary>
        /// Unique id of this notifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of this notifier to display in the user interface.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional description of this notifier to display in the user interface.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Is this notifier enabled?
        /// </summary>
        Func<bool> IsEnabled { get; }

        /// <summary>
        /// Any custom input fields for this notifier. Values entered in these fields will be used as values in <see cref="NotifierConfig.Options"/> passed to <see cref="NotifyEvent"/>.
        /// </summary>
        IEnumerable<EventNotifierOptionDefinition> Options { get; }

        /// <summary>
        /// Any custom placeholders for this notifier. Can return null.
        /// </summary>
        Dictionary<string, Func<string>> Placeholders { get; }

        /// <summary>
        /// Any custom placeholders for this notifier that won't be automatically replaced and have to be implemented yourself. Can return null.
        /// </summary>
        HashSet<string> PlaceholdersWithOnlyNames { get; }

        /// <summary>
        /// Notify using the provided config and payload values.
        /// </summary>
        /// <param name="notifierConfig">Config for this notification instance.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="payloadValues">Values from the payload. If payload is a value type or string and was stringified it will be under the 'payload' key.</param>
        /// <returns>Result text or null. If not null the latest 10 values will be stored and shown in the UI per configuration.</returns>
        Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues);
    }
}
