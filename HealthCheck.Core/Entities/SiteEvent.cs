using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Enums;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// An event that can be reported to the <see cref="ISiteEventStorage"/>.
    /// </summary>
    public class SiteEvent
    {
        /// <summary>
        /// Generated id of this event. Is set automatically from constructor.
        /// </summary>
        public Guid Id { get; set; }

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
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Event duration in minutes.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Allow merging this event with previous ones of the same type if within duration threshold.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AllowMerge { get; set; } = true;

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
        /// <param name="duration">Duration of event in minutes.</param>
        public SiteEvent(SiteEventSeverity severity, string eventTypeId, string title, string description, int duration = 1)
        {
            Id = Guid.NewGuid();
            Severity = severity;
            Timestamp = DateTime.Now;
            EventTypeId = eventTypeId;
            Title = title;
            Description = description;
            Duration = duration;
        }

        /// <summary>
        /// Create a new <see cref="SiteEvent"/> that is resolved.
        /// </summary>
        /// <param name="eventTypeId">Custom id of this type of event.</param>
        /// <param name="resolvedMessage">Resolved message.</param>
        public SiteEvent(string eventTypeId, string resolvedMessage)
        {
            EventTypeId = eventTypeId;
            Resolved = true;
            ResolvedMessage = resolvedMessage;
        }

        /// <summary>
        /// Create a new <see cref="SiteEvent"/> without any values set.
        /// </summary>
        public SiteEvent() {}

        /// <summary>
        /// Add an url related to this event.
        /// </summary>
        public SiteEvent AddRelatedLink(string title, string url)
        {
            RelatedLinks.Add(new HyperLink(title, url));
            return this;
        }

        /// <summary>
        /// Do not allow this event to be merged with previous ones.
        /// </summary>
        public SiteEvent DisallowMerge()
        {
            AllowMerge = false;
            return this;
        }

        /// <summary>
        /// Updates the last event with the matching <see cref="EventTypeId"/> if any to resolved with the given message.
        /// </summary>
        public void SetResolved(string message)
        {
            Resolved = true;
            ResolvedMessage = message;
        }

        /// <summary>
        /// Set values from the given event.
        /// </summary>
        public void SetValuesFrom(SiteEvent other, bool setId = false)
        {
            if (setId)
            {
                Id = other.Id;
            }
            Severity = other.Severity;
            Timestamp = other.Timestamp;
            EventTypeId = other.EventTypeId;
            Title = other.Title;
            Description = other.Description;
            Duration = other.Duration;
            AllowMerge = other.AllowMerge;
            RelatedLinks = other.RelatedLinks;
        }

        /// <summary>
        /// Name and resolved status of the event.
        /// </summary>
        public override string ToString() => $"{Title}{(Resolved ? $" [Resolved]" : "")}";
    }
}
