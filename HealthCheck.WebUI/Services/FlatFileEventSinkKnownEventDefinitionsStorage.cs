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
    public class FlatFileEventSinkKnownEventDefinitionsStorage : IEventSinkKnownEventDefinitionsStorage
    {
        private SimpleDataStoreWithId<KnownEventDefinition, string> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileEventSinkKnownEventDefinitionsStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileEventSinkKnownEventDefinitionsStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<KnownEventDefinition, string>(
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
    }
}
