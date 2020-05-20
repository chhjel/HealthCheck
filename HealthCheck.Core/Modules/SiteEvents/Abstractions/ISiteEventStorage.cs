using HealthCheck.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SiteEvents.Abstractions
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
        /// Update the given site event.
        /// </summary>
        Task UpdateEvent(SiteEvent siteEvent);

        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to);

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/> and <see cref="SiteEvent.AllowMerge"/> == true.
        /// </summary>
        Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId);

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/>.
        /// </summary>
        Task<SiteEvent> GetLastEventOfType(string eventTypeId);
    }
}
