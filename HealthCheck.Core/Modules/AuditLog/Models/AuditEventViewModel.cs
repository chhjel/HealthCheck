using HealthCheck.Core.Modules.AuditLog.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AuditLog.Models
{
    /// <summary>
    /// View model for a <see cref="AuditEvent"/>.
    /// </summary>
    public class AuditEventViewModel
    {
        /// <summary>
        /// Time of the event.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// What area the event is related to.
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Action that was performed.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// What object the action was performed on.
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
    }
}
