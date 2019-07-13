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
        /// <summary>
        /// Max age of events before they can become deleted.
        /// <para>Delete check is once per min(MaxEventAge, 4 hours) whenever a new event is stored.</para>
        /// </summary>
        public TimeSpan? MaxEventAge { get; set; }

        private bool CleanupEnabled => MaxEventAge != null;
        private DateTime? LastCleanup { get; set; }
        private SimpleDataStoreWithId<SiteEvent, Guid> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileSiteEventStorage"/> with the given json file path.
        /// </summary>
        public FlatFileSiteEventStorage(string filepath)
        {
            Store = new SimpleDataStoreWithId<SiteEvent, Guid>(
                filepath,
                serializer: new Func<SiteEvent, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, SiteEvent>((row) => JsonConvert.DeserializeObject<SiteEvent>(row)),
                idSelector: (e) => e.Id,
                idSetter: (e, id) => e.Id = id,
                nextIdFactory: (events, e) => Guid.NewGuid()
            );
        }

        /// <summary>
        /// Store the given event. There is a 2 second buffer delay before the item is written.
        /// </summary>
        public Task StoreEvent(SiteEvent siteEvent)
        {
            Store.InsertItem(siteEvent);
            CheckCleanup();
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
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/>.
        /// <para>Gets the last stored one with a matching id for performance reasons.</para>
        /// </summary>
        public Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId)
        {
            var item = Store.GetEnumerable(fromEnd: true)
                .FirstOrDefault(x => x.AllowMerge && x.EventTypeId == eventTypeId);
            return Task.FromResult(item);
        }

        private void CheckCleanup()
        {
            var minimumCleanupInterval = TimeSpan.FromHours(4);
            if (MaxEventAge != null && MaxEventAge < minimumCleanupInterval)
            {
                minimumCleanupInterval = MaxEventAge.Value;
            }

            // Cleanup disabled => abort
            if (!CleanupEnabled) return;
            // Less than min time since last cleanup => abort
            else if (LastCleanup != null && (DateTime.Now - LastCleanup) < minimumCleanupInterval) return;

            var threshold = DateTime.Now - MaxEventAge;
            Store.DeleteWhere(x => x.Timestamp <= threshold);

            LastCleanup = DateTime.Now;
        }
    }
}
