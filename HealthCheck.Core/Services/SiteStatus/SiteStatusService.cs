using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Services.SiteStatus
{
    /// <summary>
    /// Entry for reporting <see cref="SiteEvent"/>s.
    /// </summary>
    public class SiteStatusService
    {
        private ISiteStatusStorageService Storage { get; }

        /// <summary>
        /// Entry for reporting <see cref="SiteEvent"/>s.
        /// </summary>
        public SiteStatusService(ISiteStatusStorageService storage)
        {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Store a new <see cref="SiteEvent"/>.
        /// </summary>
        public void RegisterEvent(SiteEvent siteEvent)
        {
            Task.Run(() => Storage.StoreEvent(siteEvent));
        }

        /// <summary>
        /// Get all registered <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        public Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
            => Storage.GetEvents(from, to);
    }
}
