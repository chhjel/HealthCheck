using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Services.Models;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Manages <see cref="SiteEvent"/>s.
    /// </summary>
    public interface ISiteEventService
    {
        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to);

        /// <summary>
        /// Mark the last event with the given <paramref name="eventTypeId"/> as resolved with the given message.
        /// </summary>
        Task<bool> MarkEventAsResolved(string eventTypeId, string resolveMessage);

        /// <summary>
        /// Store a <see cref="SiteEvent"/> object.
        /// </summary>
        Task StoreEvent(SiteEvent siteEvent, SiteEventMergeOptions forcedMergeOptions = null);
    }
}