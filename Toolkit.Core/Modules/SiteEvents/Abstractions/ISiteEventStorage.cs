using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions
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
        Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to);

        /// <summary>
        /// Get the latest <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/> and <see cref="SiteEvent.AllowMerge"/> == true.
        /// </summary>
        Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId);

        /// <summary>
        /// Get the latest unresolved <see cref="SiteEvent"/> with the given <see cref="SiteEvent.EventTypeId"/>.
        /// </summary>
        Task<SiteEvent> GetLastUnresolvedEventOfType(string eventTypeId);

        /// <summary>
        /// Get the <see cref="SiteEvent"/> with the given id.
        /// </summary>
        Task<SiteEvent> GetEvent(Guid id);

        /// <summary>
        /// Get all unresolved <see cref="SiteEvent"/>s with the given <see cref="SiteEvent.EventTypeId"/>.
        /// </summary>
        Task<IEnumerable<SiteEvent>> GetUnresolvedEventsOfType(string eventTypeId);

        /// <summary>
        /// Clear all events.
        /// </summary>
        Task DeleteAllEvents();

        /// <summary>
        /// Clear single event.
        /// </summary>
        Task DeleteEvent(Guid id);
    }
}
