using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using QoDL.Toolkit.Core.Modules.EventNotifications.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Tests.Modules.EventNotifications
{
    internal static class EventNotificationsTestHelpers
    {
        internal static DefaultEventDataSink CreateEventDataSink(
            out TestEventNotifier testEventNotifier,
            out EventSinkNotificationConfig testConfig,
            Action<TestEventSinkNotificationConfigStorage> configStorageAction = null,
            Action<TestEventSinkKnownEventDefinitionsStorage> definitionStorageAction = null
        )
        {
            var configStorage = CreateConfigStorage();
            configStorageAction?.Invoke(configStorage);

            var definitionStorage = CreateDefinitionStorage();
            definitionStorageAction?.Invoke(definitionStorage);

            testEventNotifier = new TestEventNotifier();

            testConfig = configStorage.SaveConfig(CreateNotificationConfig("test_event"));
            testConfig.ClearCaches();

            var sink = new DefaultEventDataSink(configStorage, definitionStorage);
            sink.AddNotifier(testEventNotifier);

            return sink;
        }

        internal static EventSinkNotificationConfig CreateNotificationConfig(string eventIdFilter)
        {
            return new EventSinkNotificationConfig()
            {
                Id = Guid.NewGuid(),
                Enabled = true,
                NotifierConfigs = new List<NotifierConfig>()
                {
                    new NotifierConfig()
                    {
                        NotifierId = TestEventNotifier.NotifierID,
                        Options = new Dictionary<string, string>()
                    }
                },
                EventIdFilter = new EventSinkNotificationConfigFilter(){
                    CaseSensitive = true,
                    Filter = eventIdFilter,
                    MatchType = EventSinkNotificationConfigFilter.FilterMatchType.Matches
                }
            };
        }

        internal class TestEventNotifier : IEventNotifier
        {
            public const string NotifierID = "test";

            public string Id => NotifierID;
            public string Name => "Test";
            public string Description => "For unit tests";
            public Func<bool> IsEnabled => () => true;
            public Dictionary<string, Func<string>> Placeholders => null;
            public HashSet<string> PlaceholdersWithOnlyNames => null;
            public Type OptionsModelType => null;

            public List<NotifiedTestEvent> NotifiedEvents { get; set; } = new List<NotifiedTestEvent>();

            public async Task<string> NotifyEvent(NotifierConfig notifierConfig, string eventId, Dictionary<string, string> payloadValues, object optionsObject)
            {
                lock(NotifiedEvents)
                {
                    NotifiedEvents.Add(new NotifiedTestEvent
                    {
                        EventId = eventId,
                        PayloadValues = payloadValues
                    });
                }
                return await Task.FromResult<string>(null);
            }

            public class NotifiedTestEvent
            {
                public string EventId { get; set; }
                public Dictionary<string, string> PayloadValues { get; set; }
            }
        }

        internal static TestEventSinkNotificationConfigStorage CreateConfigStorage() => new();
        internal class TestEventSinkNotificationConfigStorage : IEventSinkNotificationConfigStorage
        {
            private readonly Dictionary<Guid, EventSinkNotificationConfig> _data = new();

            public EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config) => _data[config.Id] = config;
            public void DeleteConfig(Guid configId) => _data.Remove(configId);
            public IEnumerable<EventSinkNotificationConfig> GetConfigs() => _data.Values;
        }

        internal static TestEventSinkKnownEventDefinitionsStorage CreateDefinitionStorage() => new();
        internal class TestEventSinkKnownEventDefinitionsStorage : IEventSinkKnownEventDefinitionsStorage
        {
            private readonly Dictionary<string, KnownEventDefinition> _data = new();

            public IEnumerable<KnownEventDefinition> GetDefinitions() => _data.Values;

            public KnownEventDefinition InsertDefinition(KnownEventDefinition definition) => _data[definition.EventId] = definition;

            public KnownEventDefinition UpdateDefinition(KnownEventDefinition definition) => _data[definition.EventId] = definition;

            public void DeleteDefinition(string eventId) => _data.Remove(eventId);

            public void DeleteDefinitions() => _data.Clear();
        }
    }
}
