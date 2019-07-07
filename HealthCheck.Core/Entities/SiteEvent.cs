using HealthCheck.Core.Enums;
using HealthCheck.Core.Services.SiteStatus;
using System;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// An event that can be reported to the <see cref="SiteStatusService"/>.
    /// </summary>
    public class SiteEvent
    {
        /// <summary>
        /// Severity of the event.
        /// </summary>
        public SiteEventSeverity Severity { get; set; }

        /// <summary>
        /// Time of the event.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Id of this type of event.
        /// </summary>
        public string EventTypeId { get; set; }

        /// <summary>
        /// Event title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Event description.
        /// </summary>
        public string Description { get; set; }

        // related tests etc?
    }
}
