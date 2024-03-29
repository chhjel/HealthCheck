using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;

/// <summary>
/// A sink for events that should notify enabled <see cref="IEventNotifier"/>s.
/// </summary>
public interface IEventDataSink
{
    /// <summary>
    /// Get all enabled notifiers for this sink.
    /// </summary>
    IEnumerable<IEventNotifier> GetNotifiers();

    /// <summary>
    /// Get all configs.
    /// </summary>
    IEnumerable<EventSinkNotificationConfig> GetConfigs();

    /// <summary>
    /// Delete config with the given id.
    /// </summary>
    void DeleteConfig(Guid configId);

    /// <summary>
    /// Insert or update config.
    /// </summary>
    EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config);

    /// <summary>
    /// Delete event definition for the given event id.
    /// </summary>
    void DeleteDefinition(string eventId);

    /// <summary>
    /// Delete all event definitions.
    /// </summary>
    /// <returns>Number of deleted definitions.</returns>
    void DeleteDefinitions();

    /// <summary>
    /// Send an event with payload data.
    /// </summary>
    /// <param name="eventId">Id of the event that can be filtered upon.</param>
    /// <param name="payload">Optional payload.
    /// <para>It should either be stringifiable or contain public properties that can be filtered upon.</para>
    /// <para>An anonymous object works fine.</para>
    /// </param>
    /// <param name="fireAndForget">Run async and don't wait.</param>
    void RegisterEvent(string eventId, object payload = null, bool fireAndForget = true);

    /// <summary>
    /// Send an event with payload data.
    /// </summary>
    /// <param name="eventId">Id of the event that can be filtered upon.</param>
    /// <param name="payload">Optional payload.
    /// <para>It should either be stringifiable or contain public properties that can be filtered upon.</para>
    /// <para>An anonymous object works fine.</para>
    /// </param>
    /// <param name="fireAndForget">Run async and don't wait.</param>
    void RegisterEvent<T>(string eventId, T payload, bool fireAndForget = true);

    /// <summary>
    /// Get definitions of all known event ids.
    /// </summary>
    IEnumerable<KnownEventDefinition> GetKnownEventDefinitions();

    /// <summary>
    /// Get a list of custom placeholders that all notifier options should support.
    /// </summary>
    IEnumerable<string> GetPlaceholders();
}