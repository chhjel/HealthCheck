using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Models;
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

        /// <inheritdoc />
        public Task<SiteEvent> GetEvent(Guid id)
        {
            var item = Store.GetEnumerable().FirstOrDefault(x => x.Id == id);
            return Task.FromResult(item);
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
        public Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
        {
            var items = Store.GetEnumerable()
                .Where(x => x.Timestamp.ToUniversalTime() >= from.ToUniversalTime() && x.Timestamp.ToUniversalTime() <= to.ToUniversalTime())
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

        /// <inheritdoc />
        public virtual Task<SiteEvent> GetLastUnresolvedEventOfType(string eventTypeId)
        {
            var match = Store.GetEnumerable(fromEnd: true)
                    ?.Where(x => x.EventTypeId == eventTypeId && !x.Resolved)
                    ?.OrderByDescending(x => x.Timestamp)
                    ?.FirstOrDefault();
            return Task.FromResult(match);
        }

        /// <inheritdoc />
        public Task<IEnumerable<SiteEvent>> GetUnresolvedEventsOfType(string eventTypeId)
        {
            var items = Store.GetEnumerable(fromEnd: true)
                        .Where(x => x.EventTypeId == eventTypeId && !x.Resolved)
                        .AsEnumerable();
            return Task.FromResult(items);
        }

        /// <inheritdoc />
        public async Task DeleteAllEvents()
        {
            await Store.ClearDataAsync();
        }

        /// <inheritdoc />
        public Task DeleteEvent(Guid id)
        {
            Store.DeleteItem(id);
            return Task.CompletedTask;
        }
    }
}
