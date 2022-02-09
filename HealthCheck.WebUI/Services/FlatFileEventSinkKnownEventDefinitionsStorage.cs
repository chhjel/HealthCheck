using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores and retrieves <see cref="EventSinkNotificationConfig"/>s.
    /// </summary>
    public class FlatFileEventSinkKnownEventDefinitionsStorage : IEventSinkKnownEventDefinitionsStorage
    {
        private HCSimpleDataStoreWithId<KnownEventDefinition, string> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileEventSinkKnownEventDefinitionsStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileEventSinkKnownEventDefinitionsStorage(string filepath)
        {
            Store = new HCSimpleDataStoreWithId<KnownEventDefinition, string>(
                filepath,
                serializer: new Func<KnownEventDefinition, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, KnownEventDefinition>((row) => JsonConvert.DeserializeObject<KnownEventDefinition>(row)),
                idSelector: (e) => e.EventId,
                idSetter: (e, id) => e.EventId = id,
                nextIdFactory: (events, e) => e.EventId
            );
        }

        /// <summary>
        /// Get all definitions.
        /// </summary>
        public IEnumerable<KnownEventDefinition> GetDefinitions()
            => Store.GetEnumerable().ToList();

        /// <summary>
        /// Inserts the given definition.
        /// </summary>
        public KnownEventDefinition InsertDefinition(KnownEventDefinition definition)
            => Store.InsertItem(definition);

        /// <summary>
        /// Updates the given definition.
        /// </summary>
        public KnownEventDefinition UpdateDefinition(KnownEventDefinition definition)
            => Store.InsertOrUpdateItem(definition);

        /// <summary>
        /// Delete event definition for the given event id.
        /// </summary>
        public void DeleteDefinition(string eventId)
        {
            Store.DeleteItem(eventId);
        }

        /// <summary>
        /// Delete all event definitions.
        /// </summary>
        public void DeleteDefinitions()
        {
            Task.Run(() => Store.ClearDataAsync());
        }
    }
}
