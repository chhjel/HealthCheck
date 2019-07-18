using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
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
    public class FlatFileSiteEventStorage : ISiteEventStorage
    {
        private SimpleDataStoreWithId<SiteEvent, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileSiteEventStorage"/> with the given file path.
        /// </summary>
        /// <param name="filepath">Filepath to where the data will be stored.</param>
        /// <param name="maxEventAge">Max age of entries before they can become deleted. Leave at null to disable cleanup.</param>
        /// <param name="delayFirstCleanup">Delay first cleanup by the lowest of 4 hours or max event age.</param>
        public FlatFileSiteEventStorage(string filepath,
            TimeSpan? maxEventAge = null,
            bool delayFirstCleanup = true)
        {
            Store = new SimpleDataStoreWithId<SiteEvent, Guid>(
                filepath,
                serializer: new Func<SiteEvent, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, SiteEvent>((row) => JsonConvert.DeserializeObject<SiteEvent>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );

            if (maxEventAge != null)
            {
                var minimumCleanupInterval = TimeSpan.FromHours(4);
                Store.RetentionOptions = new StorageRetentionOptions<SiteEvent>(
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
        public Task StoreEvent(SiteEvent siteEvent)
        {
            Store.InsertItem(siteEvent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update the given site event.
        /// </summary>
        public Task UpdateEvent(SiteEvent siteEvent)
        {
            Store.InsertOrUpdateItem(siteEvent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get stored events within the given threshold.
        /// </summary>
        public Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
        {
            var items = Store.GetEnumerable()
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .ToList();

            return Task.FromResult(items);
        }

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/> and <see cref="SiteEvent.AllowMerge"/> == true.
        /// <para>Gets the last stored one with a matching id for performance reasons.</para>
        /// </summary>
        public Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId)
        {
            var item = Store.GetEnumerable(fromEnd: true)
                .FirstOrDefault(x => x.AllowMerge && x.EventTypeId == eventTypeId);
            return Task.FromResult(item);
        }

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/>.
        /// </summary>
        public Task<SiteEvent> GetLastEventOfType(string eventTypeId)
        {
            var item = Store.GetEnumerable(fromEnd: true)
                .FirstOrDefault(x => x.EventTypeId == eventTypeId);
            return Task.FromResult(item);
        }
    }
}
