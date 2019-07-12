using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Enums;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// An event that can be reported to the <see cref="ISiteEventService"/>.
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

        /// <summary>
        /// Any urls to related things.
        /// </summary>
        public List<HyperLink> RelatedLinks { get; set; } = new List<HyperLink>();

        /// <summary>
        /// Create a new <see cref="SiteEvent"/>.
        /// </summary>
        /// <param name="severity">Severity of the event.</param>
        /// <param name="eventTypeId">Custom id of this type of event.</param>
        /// <param name="title">Title of the event.</param>
        /// <param name="description">Description of the event.</param>
        public SiteEvent(SiteEventSeverity severity, string eventTypeId, string title, string description)
        {
            Severity = severity;
            Timestamp = DateTime.Now;
            EventTypeId = eventTypeId;
            Title = title;
            Description = description;
        }

        /// <summary>
        /// Add an url related to this event.
        /// </summary>
        public SiteEvent AddRelatedLink(string title, string url)
        {
            RelatedLinks.Add(new HyperLink(title, url));
            return this;
        }
    }
}
