using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;

namespace HealthCheck.Core.Services.Storage
{
    /// <summary>
    /// Stores in memory only.
    /// </summary>
    public class MemorySiteEventStorage : ISiteEventStorage
    {
        private readonly ConcurrentBag<SiteEvent> Items = new ConcurrentBag<SiteEvent>();

        /// <summary>
        /// Store the given event in memory.
        /// </summary>
        public Task StoreEvent(SiteEvent siteEvent)
        {
            Items.Add(siteEvent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update the given site event.
        /// </summary>
        public Task UpdateEvent(SiteEvent siteEvent)
        {
            var items = Items.Where(x => x.Id == siteEvent.Id);
            foreach (var item in items)
            {
                item.SetValuesFrom(siteEvent);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get some events from memory.
        /// </summary>
        public Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
             => Task.FromResult(Items.Where(x => x.Timestamp >= from && x.Timestamp <= to).ToList());

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/>.
        /// </summary>
        public Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId)
        {
            var item = Items
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefault(x => x.AllowMerge && x.EventTypeId == eventTypeId);
            return Task.FromResult(item);
        }
    }
}
