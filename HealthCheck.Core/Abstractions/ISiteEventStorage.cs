using HealthCheck.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Storage for <see cref="SiteEvent"/> objects.
    /// </summary>
    public interface ISiteEventStorage
    {
        /// <summary>
        /// Store a <see cref="SiteEvent"/> object.
        /// </summary>
        Task StoreEvent(SiteEvent siteEvent);

        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to);
    }
}
