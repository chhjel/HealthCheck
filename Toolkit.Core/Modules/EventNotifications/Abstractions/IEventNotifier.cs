using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;

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
    /// Any custom placeholders for this notifier. Can return null.
    /// </summary>
    Dictionary<string, Func<string>> Placeholders { get; }

    /// <summary>
    /// Any custom placeholders for this notifier that won't be automatically replaced and have to be implemented yourself. Can return null.
    /// </summary>
    HashSet<string> PlaceholdersWithOnlyNames { get; }

    /// <summary>
    /// To add any options to this notifier set this property to a type of your options model.
    /// <para>Any public get/set properties in this model decorated with [EventNotifierOption] will be shown in the interface.</para>
    /// <para>Values can be retrieved in the options object passed to <see cref="NotifyEvent"/>.</para>
    /// </summary>
    public Type OptionsModelType { get; }

    /// <summary>
    /// Notify using the provided config and payload values.
    /// </summary>
    /// <param name="notifierConfig">Config for this notification instance.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="payloadValues">Values from the payload. If payload is a value type or string and was stringified it will be under the 'payload' key.</param>
    /// <param name="optionsObject">An instance of the type <see cref="OptionsModelType"/> if <see cref="OptionsModelType"/> is not null.</param>
    /// <returns>Result text or null. If not null the latest 10 values will be stored and shown in the UI per configuration.</returns>
    Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, object optionsObject);
}
