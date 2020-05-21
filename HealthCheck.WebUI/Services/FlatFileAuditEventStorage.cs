using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores events to the given file.
    /// </summary>
    public class FlatFileAuditEventStorage : IAuditEventStorage
    {
        private SimpleDataStore<AuditEvent> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileAuditEventStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        /// <param name="maxEventAge">Max age of entries before they can become deleted. Leave at null to disable cleanup.</param>
        /// <param name="delayFirstCleanup">Delay first cleanup by the lowest of 4 hours or max event age.</param>
        public FlatFileAuditEventStorage(string filepath,
            TimeSpan? maxEventAge = null,
            bool delayFirstCleanup = true)
        {
            Store = new SimpleDataStore<AuditEvent>(
                filepath,
                serializer: new Func<AuditEvent, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, AuditEvent>((row) => JsonConvert.DeserializeObject<AuditEvent>(row))
            );

            if (maxEventAge != null)
            {
                var minimumCleanupInterval = TimeSpan.FromHours(4);
                Store.RetentionOptions = new StorageRetentionOptions<AuditEvent>(
                    (item) => item.Timestamp,
                    maxAge: maxEventAge.Value,
                    minimumCleanupInterval: (maxEventAge.Value < minimumCleanupInterval) ? maxEventAge.Value : minimumCleanupInterval,
                    delayFirstCleanup: delayFirstCleanup
                );
            }
        }

        /// <summary>
        /// Store the given event. There is a 2 second buffer delay before the item is written.
        /// </summary>
        public Task StoreEvent(AuditEvent siteEvent)
        {
            Store.InsertItem(siteEvent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get stored events within the given threshold.
        /// </summary>
        public Task<List<AuditEvent>> GetEvents(DateTime from, DateTime to)
        {
            var items = Store.GetEnumerable()
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .ToList();

            return Task.FromResult(items);
        }
    }
}
