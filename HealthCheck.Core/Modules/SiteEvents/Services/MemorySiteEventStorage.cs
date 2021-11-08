using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SiteEvents.Services
{
    /// <summary>
    /// Stores in memory only.
    /// </summary>
    public class MemorySiteEventStorage : ISiteEventStorage
    {
        private ConcurrentBag<SiteEvent> Items = new ConcurrentBag<SiteEvent>();
        private readonly object _lock = new object();

        /// <summary>
        /// Store the given event in memory.
        /// </summary>
        public Task StoreEvent(SiteEvent siteEvent)
        {
            lock (_lock)
            {
                Items.Add(siteEvent);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Update the given site event.
        /// </summary>
        public Task UpdateEvent(SiteEvent siteEvent)
        {
            lock (_lock)
            {
                var items = Items.Where(x => x.Id == siteEvent.Id);
                foreach (var item in items)
                {
                    item.SetValuesFrom(siteEvent);
                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Get some events from memory.
        /// </summary>
        public Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
        {
            lock (_lock)
            {
                return Task.FromResult(Items.Where(x => x.Timestamp.ToUniversalTime() >= from.ToUniversalTime() && x.Timestamp.ToUniversalTime() <= to.ToUniversalTime()).ToList());
            }
        }

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/> and <see cref="SiteEvent.AllowMerge"/> == true.
        /// </summary>
        public Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId)
        {
            lock (_lock)
            {
                var item = Items
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefault(x => x.AllowMerge && x.EventTypeId == eventTypeId);
                return Task.FromResult(item);
            }
        }

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/>.
        /// </summary>
        public Task<SiteEvent> GetLastEventOfType(string eventTypeId)
        {
            lock (_lock)
            {
                var item = Items
                    .OrderByDescending(x => x.Timestamp)
                    .FirstOrDefault(x => x.EventTypeId == eventTypeId);
                return Task.FromResult(item);
            }
        }

        /// <inheritdoc />
        public Task DeleteAllEvents()
        {
            lock (_lock)
            {
                Items = new ConcurrentBag<SiteEvent>();
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteEvent(Guid id)
        {
            lock (_lock)
            {
                Items = new ConcurrentBag<SiteEvent>(Items.Where(x => x.Id != id));
            }
            return Task.CompletedTask;
        }
    }
}
