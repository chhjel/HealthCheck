using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Services
{
    /// <summary>
    /// Manages <see cref="SiteEvent"/>s.
    /// </summary>
    public class SiteEventService
    {
        private ISiteEventStorage Storage { get; set; }

        /// <summary>
        /// Create a new <see cref="SiteEventService"/> that manages <see cref="SiteEvent"/>s.
        /// </summary>
        /// <param name="storage">Implementation for event storage.</param>
        public SiteEventService(ISiteEventStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Store a <see cref="SiteEvent"/> object. Respects any event merge options.
        /// </summary>
        public async Task StoreEvent(SiteEvent siteEvent)
        {
            // ToDo merge
            await Storage.StoreEvent(siteEvent);
        }

        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        public async Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
            => await Storage.GetEvents(from, to);
    }
}
