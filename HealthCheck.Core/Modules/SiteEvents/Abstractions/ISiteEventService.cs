using HealthCheck.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SiteEvents.Abstractions
{
    /// <summary>
    /// Manages <see cref="SiteEvent"/>s.
    /// </summary>
    public interface ISiteEventService
    {
        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to);

        /// <summary>
        /// Get all stored unresolved <see cref="SiteEvent"/>s objects, optionally with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        Task<List<SiteEvent>> GetUnresolvedEvents(DateTimeOffset? from = null, DateTimeOffset? to = null);

        /// <summary>
        /// Mark the last event with the given <paramref name="eventTypeId"/> as resolved with the given message.
        /// </summary>
        Task<bool> MarkEventAsResolved(string eventTypeId, string resolveMessage, Action<SiteEvent> config = null);

        /// <summary>
        /// Store a <see cref="SiteEvent"/> object.
        /// </summary>
        Task StoreEvent(SiteEvent siteEvent, SiteEventMergeOptions forcedMergeOptions = null);

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