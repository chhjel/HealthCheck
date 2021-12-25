using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Attributes;
using HealthCheck.Core.Modules.EventNotifications.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.EventNotifications.Services
{
    /// <summary>
    /// Collects generic events, runs them through any custom filters and notifies any connected <see cref="IEventNotifier"/>
    /// </summary>
    public class DefaultEventDataSink : IEventDataSink
    {
        /// <summary>
        /// Only store this amount of event definitions. Defaults to 1000.
        /// </summary>
        public int EventDefinitionSizeLimit { get; set; } = 1000;

        /// <summary>
        /// If disabled the sink will ignore any attempts to register events.
        /// <para>Enabled by default. Null value/exception = false.</para>
        /// </summary>
        public Func<bool> IsEnabled { get; set; } = () => true;

        private IEventSinkNotificationConfigStorage EventSinkNotificationConfigStorage { get; set; }
        private IEventSinkKnownEventDefinitionsStorage EventSinkKnownEventDefinitionsStorage { get; }
        private List<IEventNotifier> Notifiers { get; set; } = new List<IEventNotifier>();

        private Dictionary<string, Func<string>> Placeholders { get; set; } = new Dictionary<string, Func<string>>();

        private static readonly object _cacheUpdateLock = new();
        private static Dictionary<string, KnownEventDefinition> KnownEventDefinitionsCache { get; set; }
        private static List<KnownEventDefinition> KnownEventDefinitionsListCache { get; set; }
        private static int KnownEventDefinitionsCacheSize { get; set; }

        /// <summary>
        /// Collects generic events, runs them through any custom filters and notifies any connected <see cref="IEventNotifier"/>
        /// </summary>
        public DefaultEventDataSink(
            IEventSinkNotificationConfigStorage eventSinkNotificationConfigStorage,
            IEventSinkKnownEventDefinitionsStorage eventSinkKnownEventDefinitionsStorage
        )
        {
            EventSinkNotificationConfigStorage = eventSinkNotificationConfigStorage;
            EventSinkKnownEventDefinitionsStorage = eventSinkKnownEventDefinitionsStorage;

            EnsureDefinitionsAreLoaded();
        }

        /// <summary>
        /// Get definitionss of all known event ids.
        /// </summary>
        public IEnumerable<KnownEventDefinition> GetKnownEventDefinitions()
        {
            lock (_cacheUpdateLock) {
                return KnownEventDefinitionsListCache;
            }
        }

        /// <summary>
        /// Get a list of custom placeholders that all notifier options support.
        /// </summary>
        public IEnumerable<string> GetPlaceholders() => Placeholders.Keys;

        /// <summary>
        /// Register a custom placeholder that all notifier options will support.
        /// </summary>
        public DefaultEventDataSink AddPlaceholder(string key, Func<string> valueFactory)
        {
            Placeholders[key] = valueFactory;
            return this;
        }

        /// <summary>
        /// Register custom placeholders that all notifier options will support.
        /// </summary>
        public DefaultEventDataSink AddPlaceholders(Dictionary<string, Func<string>> placeholderFactories)
        {
            foreach(var kvp in placeholderFactories)
            {
                AddPlaceholder(kvp.Key, kvp.Value);
            }
            return this;
        }

        /// <summary>
        /// Get all configs.
        /// </summary>
        public IEnumerable<EventSinkNotificationConfig> GetConfigs()
            => EventSinkNotificationConfigStorage.GetConfigs();

        /// <summary>
        /// Delete config with the given id.
        /// </summary>
        public void DeleteConfig(Guid configId)
            => EventSinkNotificationConfigStorage.DeleteConfig(configId);

        /// <summary>
        /// Insert or update config.
        /// </summary>
        public EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config)
            => EventSinkNotificationConfigStorage.SaveConfig(config);

        /// <summary>
        /// Delete event definition for the given event id.
        /// </summary>
        public void DeleteDefinition(string eventId)
        {
            EventSinkKnownEventDefinitionsStorage.DeleteDefinition(eventId);
            DeleteDefinitionFromCache(eventId);
        }

        /// <summary>
        /// Delete all event definitions.
        /// </summary>
        public void DeleteDefinitions()
        {
            EventSinkKnownEventDefinitionsStorage.DeleteDefinitions();
            ClearDefinitionCache();
        }

        /// <summary>
        /// Get all enabled notifiers for this sink.
        /// </summary>
        public IEnumerable<IEventNotifier> GetNotifiers()
            => Notifiers.Where(x => x.IsEnabled?.Invoke() == true);

        /// <summary>
        /// Register a notifier that events can be sent to.
        /// </summary>
        public DefaultEventDataSink AddNotifier(IEventNotifier notifier)
        {
            Notifiers.Add(notifier);
            return this;
        }

        /// <summary>
        /// Register a set of notifiers that events can be sent to.
        /// </summary>
        public DefaultEventDataSink AddNotifiers(IEnumerable<IEventNotifier> notifiers)
        {
            Notifiers.AddRange(notifiers);
            return this;
        }

        /// <summary>
        /// If disabled the sink will ignore any attempts to register events.
        /// <para>Enabled by default. Null value/exception = false.</para>
        /// </summary>
        public DefaultEventDataSink SetIsEnabled(Func<bool> isEnabledFunc)
        {
            IsEnabled = isEnabledFunc;
            return this;
        }

        /// <summary>
        /// Send an event with payload data.
        /// </summary>
        /// <param name="eventId">Id of the event that can be filtered upon.</param>
        /// <param name="payload">Optional payload.
        /// <para>It should either be stringifiable or contain public properties that can be filtered upon.</para>
        /// <para>An anonymous object works fine.</para>
        /// </param>
        /// <param name="fireAndForget">Run async and don't wait.</param>
        public void RegisterEvent<T>(string eventId, T payload, bool fireAndForget = true)
            => RegisterEvent(eventId, payload as object, fireAndForget);

        /// <summary>
        /// Send an event with payload data.
        /// </summary>
        /// <param name="eventId">Id of the event that can be filtered upon.</param>
        /// <param name="payload">Optional payload.
        /// <para>It should either be stringifiable or contain public properties that can be filtered upon.</para>
        /// <para>An anonymous object works fine.</para>
        /// </param>
        /// <param name="fireAndForget">Run async and don't wait.</param>
        public void RegisterEvent(string eventId, object payload = null, bool fireAndForget = true)
        {
            if (fireAndForget)
            {
                Task.Run(() => RegisterEventInternal(eventId, payload, fireAndForget));
            }
            else
            {
                RegisterEventInternal(eventId, payload, fireAndForget);
            }
        }

        private void RegisterEventInternal(string eventId, object payload = null, bool fireAndForget = true)
        {
            try
            {
                if (IsEnabled?.Invoke() != true)
                {
                    return;
                }
            }
            catch (Exception) { return; }

            var payloadIsComplex = false;
            var payloadProperties = new Dictionary<string, string>();
            var stringifiedPayload = payload?.ToString();

            if (payload != null)
            {
                var type = payload.GetType();
                payloadIsComplex = !type.IsValueType && type != typeof(string);

                if (payloadIsComplex)
                {
                    var props = payload.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        try
                        {
                            payloadProperties[prop.Name] = prop.GetValue(payload)?.ToString();
                        }
                        catch (Exception) { /* Ignore error here */ }
                    }
                }
                else
                {
                    payloadProperties["payload"] = stringifiedPayload;
                }
            }

            // Create definition of payload if not existing
            EnsureDefinition(eventId, payloadProperties, payloadIsComplex);

            // Check that we have any enabled notifiers
            var notifiers = Notifiers?.Where(x => x.IsEnabled())?.ToList();
            if (notifiers == null || notifiers.Count == 0)
            {
                return;
            }

            // Check that we have any enabled configs
            var configs = EventSinkNotificationConfigStorage.GetConfigs().ToList()
                ?.Where(x => x.Enabled && x.NotifierConfigs.Any(nConfig => notifiers.Any(n => n.Id == nConfig.NotifierId)))
                ?.ToList();
            if (configs == null || configs.Count == 0)
            {
                return;
            }

            foreach (var config in configs)
            {
                var notifiersForThisConfig = notifiers
                    .Where(x => config.NotifierConfigs.Any(nConfig => nConfig.NotifierId == x.Id))
                    .ToList();
                if (!notifiersForThisConfig.Any())
                {
                    continue;
                }

                if (config.IsAllowed(eventId, payloadIsComplex, stringifiedPayload, payloadProperties))
                {
                    foreach (var notifier in notifiersForThisConfig)
                    {
                        var notifierConfig = config.NotifierConfigs.FirstOrDefault(x => x.NotifierId == notifier.Id);
                        if (notifierConfig == null)
                        {
                            continue;
                        }

                        var task = Task.Run(async () =>
                        {
                            var result = await NotifyEventAsync(notifier, notifierConfig, eventId, payloadProperties);
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                result = $"{DateTimeOffset.Now:ddd d. MMMM @ HH:mm:ss} - {result}";
                            }
                            lock (_cacheUpdateLock)
                            {
                                OnEventNotified(config, result);
                            }
                        });

                        if (!fireAndForget)
                        {
                            task.ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                    }
                }
            }
        }

        private void EnsureDefinitionsAreLoaded()
        {
            lock (_cacheUpdateLock)
            {
                if (KnownEventDefinitionsListCache == null)
                {
                    KnownEventDefinitionsCache = new Dictionary<string, KnownEventDefinition>();
                    KnownEventDefinitionsListCache = new List<KnownEventDefinition>();

                    var defs = EventSinkKnownEventDefinitionsStorage?.GetDefinitions()?.Where(x => x?.EventId != null) ?? Enumerable.Empty<KnownEventDefinition>();
                    foreach (var def in defs)
                    {
                        KnownEventDefinitionsCache[def.EventId] = def;
                        KnownEventDefinitionsListCache.Add(def);
                    }
                }
            }
        }

        private void ClearDefinitionCache()
        {
            lock (_cacheUpdateLock)
            {
                KnownEventDefinitionsListCache?.Clear();
                KnownEventDefinitionsCache?.Clear();
                KnownEventDefinitionsCacheSize = 0;
            }
        }

        private void DeleteDefinitionFromCache(string eventId)
        {
            lock (_cacheUpdateLock)
            {
                var index = KnownEventDefinitionsListCache.FindIndex(x => x.EventId == eventId);
                if (index == -1)
                {
                    return;
                }

                KnownEventDefinitionsListCache.RemoveAt(index);
                KnownEventDefinitionsCache.Remove(eventId);
                KnownEventDefinitionsCacheSize--;
            }
        }

        private void EnsureDefinition(string eventId, Dictionary<string, string> payloadProperties, bool payloadIsComplex)
        {
            lock(_cacheUpdateLock)
            {
                var idIsKnown = KnownEventDefinitionsCache.ContainsKey(eventId);
                var countLimitIsReached = KnownEventDefinitionsCacheSize > EventDefinitionSizeLimit;

                // Do not insert new when count limit is reached
                if (!idIsKnown && countLimitIsReached)
                {
                    return;
                }

                // If already known, diff keys if complex.
                if (idIsKnown && payloadIsComplex)
                {
                    var existingDef = KnownEventDefinitionsCache[eventId];
                    if (existingDef.PayloadProperties != null)
                    {
                        var newKeys = payloadProperties.Keys.Except(existingDef.PayloadProperties);

                        // Abort if no new keys
                        // * Only add new ones & don't remove any missing,
                        //   to prevent constant updates if there's calls with different payloads on the same id.
                        if (!newKeys.Any())
                        {
                            return;
                        }

                        // Update existing def with new payload parameters
                        existingDef.PayloadProperties.AddRange(newKeys);
                        EventSinkKnownEventDefinitionsStorage.UpdateDefinition(existingDef);
                        return;
                    }
                }

                var newDefinition = new KnownEventDefinition()
                {
                    EventId = eventId,
                    IsStringified = !payloadIsComplex,
                    PayloadProperties = payloadProperties.Select(x => x.Key).ToList()
                };

                KnownEventDefinitionsCache[eventId] = newDefinition;
                KnownEventDefinitionsListCache.Add(newDefinition);
                KnownEventDefinitionsCacheSize++;

                EventSinkKnownEventDefinitionsStorage.InsertDefinition(newDefinition);
            }
        }

        private void OnEventNotified(EventSinkNotificationConfig config, string result)
        {
            config.LastNotifiedAt = DateTimeOffset.Now;

            if (config.NotificationCountLimit != null)
            {
                config.NotificationCountLimit--;
                if (config.NotificationCountLimit < 0) config.NotificationCountLimit = 0;
            }

            if (config.LimitHasBeenReached())
            {
                config.Enabled = false;
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                config.LatestResults.Insert(0, result);
                if (config.LatestResults.Count > 10)
                {
                    config.LatestResults = config.LatestResults.Take(10).ToList();
                }
            }

            EventSinkNotificationConfigStorage.SaveConfig(config);
        }

        private async Task<string> NotifyEventAsync(IEventNotifier notifier,
            NotifierConfig notifierConfig,
            string eventId, Dictionary<string, string> payloadValues)
        {
            string result;

            try
            {
                var placeholders = CreatePlaceholdersDictionary(payloadValues, notifier);
                var options = CreateOptionsObject(notifier, notifierConfig, placeholders);

                result = await notifier.NotifyEvent(notifierConfig, eventId, payloadValues, options);
            }
            catch (Exception ex)
            {
                result = $"Failed to notify through '{notifier.GetType().Name}' with the error '{ex.Message}'.";
            }

            return result;
        }

        internal static object CreateOptionsObject(IEventNotifier notifier, NotifierConfig notifierConfig, Dictionary<string, string> placeholders)
        {
            var type = notifier.OptionsModelType;
            if (type == null)
            {
                return null;
            }

            object instance = HCValueConversionUtils.ConvertInputModel(type, notifierConfig.Options,
                placeholders: placeholders,
                placeholderTransformerFactory: (prop, attr) =>
                {
                    var transformationMethod = (attr as EventNotifierOptionAttribute).PlaceholderTransformerMethod;
                    if (!string.IsNullOrWhiteSpace(transformationMethod))
                    {
                        var methodInfo = type.GetMethod(transformationMethod, BindingFlags.Static | BindingFlags.Public);
                        if (methodInfo == null)
                            throw new ArgumentException($"Placeholder transformation method '{transformationMethod}' was not found on '{type.Name}'.");

                        return (value) => methodInfo.Invoke(null, new[] { value }) as string;
                    }
                    return null;
                });

            return instance;
        }

        private Dictionary<string, string> CreatePlaceholdersDictionary(Dictionary<string, string> payloadValues, IEventNotifier notifier)
        {
            var dict = new Dictionary<string, string>();

            if (Placeholders != null)
            {
                foreach (var kvp in Placeholders)
                {
                    dict[kvp.Key.ToUpper().Trim()] = kvp.Value?.Invoke() ?? "";
                }
            }

            if (notifier.Placeholders != null)
            {
                foreach (var kvp in notifier.Placeholders)
                {
                    dict[kvp.Key.ToUpper().Trim()] = kvp.Value?.Invoke() ?? "";
                }
            }

            if (payloadValues != null)
            {
                foreach (var kvp in payloadValues)
                {
                    dict[kvp.Key.ToUpper().Trim()] = kvp.Value;
                }
            }

            return dict;
        }
    }
}
