using HealthCheck.Core.Enums;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// An action a user performed that can be logged in the audit log.
    /// </summary>
    public class AuditEvent
    {
        /// <summary>
        /// Time of the event.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// What area the event is related to.
        /// </summary>
        public AuditEventArea Area { get; set; }

        /// <summary>
        /// Event title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Event subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Event details.
        /// </summary>
        public List<KeyValuePair<string, string>> Details { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Id of the related user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Username of the related user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The access roles of the user at the point in time when the event was created.
        /// </summary>
        public List<string> UserAccessRoles { get; set; }

        /// <summary>
        /// Create a new empty <see cref="AuditEvent"/>.
        /// </summary>
        public AuditEvent() { }

        /// <summary>
        /// Create a new <see cref="AuditEvent"/>.
        /// </summary>
        public AuditEvent(DateTime timestamp, AuditEventArea area, string title, string subject, string userId, string userName, List<string> userAccessRoles)
        {
            Timestamp = timestamp;
            Area = area;
            Title = title;
            Subject = subject;
            UserId = userId;
            UserName = userName;
            UserAccessRoles = userAccessRoles;
        }

        /// <summary>
        /// Add a detail to this event.
        /// </summary>
        public AuditEvent AddDetail(string name, string value)
        {
            Details.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }
    }
}
