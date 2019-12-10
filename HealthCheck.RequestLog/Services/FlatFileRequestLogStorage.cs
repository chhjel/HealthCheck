#if NETFULL
using HealthCheck.RequestLog.Abstractions;
using HealthCheck.Core.Modules.RequestLog.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.RequestLog.Services
{
    /// <summary>
    /// Stores entries in the test log.
    /// </summary>
    public class FlatFileRequestLogStorage : IRequestLogStorage
    {
        private SimpleDataStoreWithId<LoggedEndpointDefinition, Guid> Store { get; }

        /// <summary>
        /// Create a new <see cref="FlatFileRequestLogStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileRequestLogStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<LoggedEndpointDefinition, Guid>(
                filepath,
                serializer: new Func<LoggedEndpointDefinition, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, LoggedEndpointDefinition>((row) => JsonConvert.DeserializeObject<LoggedEndpointDefinition>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );
        }

        /// <summary>
        /// Get the first entry with the given endpoint id.
        /// </summary>
        public LoggedEndpointDefinition GetEntryWithEndpointId(string endpointId)
            => Store.GetEnumerable().FirstOrDefault(x => x.EndpointId == endpointId);

        /// <summary>
        /// Insert a new item or update existing with the entry id.
        /// </summary>
        public void InsertOrUpdate(LoggedEndpointDefinition entry)
            => Store.InsertOrUpdateItem(entry);

        /// <summary>
        /// Insert a new item.
        /// </summary>
        public void Insert(LoggedEndpointDefinition entry)
            => Store.InsertItem(entry);

        /// <summary>
        /// Get all items.
        /// </summary>
        public List<LoggedEndpointDefinition> GetAll()
            => Store.GetEnumerable().ToList();

        /// <summary>
        /// Clear all calls/errors, and optionally definitions.
        /// </summary>
        public async Task ClearAll(bool includeDefinitions = false)
        {
            if (includeDefinitions)
            {
                await Store.ClearDataAsync();
                return;
            }

            Store.UpdateWhere(
                x => x.Calls.Any() || x.Errors.Any(),
                (entry) =>
                {
                    entry.Calls.Clear();
                    entry.Errors.Clear();
                    return entry;
                }
            );
        }
    }
}
#endif
