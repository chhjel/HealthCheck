using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SiteEvents.Models
{
    /// <summary>
    /// View model for a <see cref="SiteEvent"/>.
    /// </summary>
    public class SiteEventViewModel
    {
        /// <summary>
        /// Generated id of this event. Is set automatically from constructor.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Severity of the event.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Severity code of the event.
        /// </summary>
        public int SeverityCode { get; set; }

        /// <summary>
        /// Time of the event.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// End of the event.
        /// </summary>
        public DateTimeOffset EndTime { get; set; }

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

        /// <summary>
        /// Event duration in minutes.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Any urls to related things.
        /// </summary>
        public List<HCSEHyperLinkViewModel> RelatedLinks { get; set; }

        /// <summary>
        /// Details for developers.
        /// </summary>
        public string DeveloperDetails { get; set; }

        /// <summary>
        /// True when the event is resolved.
        /// </summary>
        public bool Resolved { get; set; }

        /// <summary>
        /// Message that is displayed when the event is resolved.
        /// </summary>
        public string ResolvedMessage { get; set; }

        /// <summary>
        /// Resolved at timestamp.
        /// </summary>
        public DateTimeOffset? ResolvedAt { get; set; }
    }
}
