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
    public class MemorySiteEventService : ISiteEventService
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
        /// Get some events from memory.
        /// </summary>
        public Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
             => Task.FromResult(Items.Where(x => x.Timestamp >= from && x.Timestamp <= to).ToList());
    }
}
