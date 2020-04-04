using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores and retrieves <see cref="EventSinkNotificationConfig"/>s.
    /// </summary>
    public class FlatFileEventSinkNotificationConfigStorage : IEventSinkNotificationConfigStorage
    {
        private SimpleDataStoreWithId<EventSinkNotificationConfig, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileEventSinkNotificationConfigStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileEventSinkNotificationConfigStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<EventSinkNotificationConfig, Guid>(
                filepath,
                serializer: new Func<EventSinkNotificationConfig, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, EventSinkNotificationConfig>((row) => JsonConvert.DeserializeObject<EventSinkNotificationConfig>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );
        }

        /// <summary>
        /// Get all configs.
        /// </summary>
        public IEnumerable<EventSinkNotificationConfig> GetConfigs()
            => Store.GetEnumerable().ToList();

        /// <summary>
        /// Inserts or updates the given config.
        /// </summary>
        public EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config)
            => Store.InsertOrUpdateItem(config, (old) =>
            {
                config.LatestResults = old.LatestResults;
                return config;
            });

        /// <summary>
        /// Deletes the config with the given id.
        /// </summary>
        public void DeleteConfig(Guid configId)
            => Store.DeleteItem(configId);
    }
}
