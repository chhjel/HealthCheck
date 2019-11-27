#if NETFULL
using HealthCheck.ActionLog.Abstractions;
using HealthCheck.Core.Modules.ActionsTestLog.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.ActionLog.Services
{
    /// <summary>
    /// Stores entries in the test log.
    /// </summary>
    public class FlatFileTestLogStorage : ITestLogStorage
    {
        private SimpleDataStoreWithId<LoggedActionEntry, Guid> Store { get; }

        /// <summary>
        /// Create a new <see cref="FlatFileTestLogStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        public FlatFileTestLogStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<LoggedActionEntry, Guid>(
                filepath,
                serializer: new Func<LoggedActionEntry, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, LoggedActionEntry>((row) => JsonConvert.DeserializeObject<LoggedActionEntry>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );
        }

        /// <summary>
        /// Get the first entry with the given endpoint id.
        /// </summary>
        public LoggedActionEntry GetEntryWithEndpointId(string endpointId)
            => Store.GetEnumerable().FirstOrDefault(x => x.EndpointId == endpointId);

        /// <summary>
        /// Insert a new item or update existing with the entry id.
        /// </summary>
        public void InsertOrUpdate(LoggedActionEntry entry)
            => Store.InsertOrUpdateItem(entry);

        /// <summary>
        /// Insert a new item.
        /// </summary>
        public void Insert(LoggedActionEntry entry)
            => Store.InsertItem(entry);

        /// <summary>
        /// Get all items.
        /// </summary>
        public List<LoggedActionEntry> GetAll()
            => Store.GetEnumerable().ToList();
        
        /// <summary>
        /// Clear all litems.
        /// </summary>
        public async Task ClearAll()
            => await Store.ClearDataAsync();
    }
}
#endif
