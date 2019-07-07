using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using System;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// View model for a <see cref="SiteEvent"/>.
    /// </summary>
    public class SiteEventViewModel
    {
        /// <summary>
        /// Severity of the event.
        /// </summary>
        public SiteEventSeverity Severity { get; set; }

        /// <summary>
        /// Severity code of the event.
        /// </summary>
        public int SeverityCode { get; set; }

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
    }
}
