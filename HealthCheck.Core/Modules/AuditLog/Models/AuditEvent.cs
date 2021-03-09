using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.AuditLog.Models
{
    /// <summary>
    /// An action a user performed that can be logged in the audit log.
    /// </summary>
    public class AuditEvent
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
        /// Event blob ids.
        /// </summary>
        public List<KeyValuePair<string, Guid>> BlobIds { get; private set; } = new List<KeyValuePair<string, Guid>>();

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

        internal List<KeyValuePair<string, string>> _blobContents = new List<KeyValuePair<string, string>>();
        private bool _clientConnectionDetailsAdded = false;

        /// <summary>
        /// Create a new empty <see cref="AuditEvent"/>.
        /// </summary>
        public AuditEvent() { }

        /// <summary>
        /// Create a new <see cref="AuditEvent"/>.
        /// </summary>
        public AuditEvent(DateTimeOffset timestamp, string area, string title, string subject, string userId, string userName, List<string> userAccessRoles)
        {
            Timestamp = timestamp;
            Area = area;
            Action = title;
            Subject = subject;
            UserId = userId;
            UserName = userName;
            UserAccessRoles = userAccessRoles;
        }

        /// <summary>
        /// Add a detail to this event.
        /// </summary>
        public AuditEvent AddDetail(string name, string value, bool onlyIfNotNull = false)
        {
            if (onlyIfNotNull && value == null)
            {
                return this;
            }

            Details.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        /// <summary>
        /// Add a blob to this event. Will only be stored if an <see cref="IAuditBlobStorage"/> is in use.
        /// </summary>
        public AuditEvent AddBlob(string name, string contents, bool onlyIfNotNullOrEmpty = true, bool onlyIfThisIsTrue = true)
        {
            if (!onlyIfThisIsTrue)
            {
                return this;
            }

            if (onlyIfNotNullOrEmpty && string.IsNullOrWhiteSpace(contents))
            {
                return this;
            }

            _blobContents.Add(new KeyValuePair<string, string>(name, contents ?? ""));
            return this;
        }

        /// <summary>
        /// Add ip address and user agent data.
        /// </summary>
        public AuditEvent AddClientConnectionDetails(HealthCheckModuleContext context)
        {
            if (!_clientConnectionDetailsAdded)
            {
                AddDetail("Client IP", context.Request.ClientIP);
                AddDetail("Client User-Agent", context.Request.UserAgent);
                _clientConnectionDetailsAdded = true;
            }

            return this;
        }

        /// <summary>
        /// Will not return values when this event is retrieved from the audit store..
        /// </summary>
        public List<KeyValuePair<string, string>> GetBlobs()
        {
            return _blobContents ?? new List<KeyValuePair<string, string>>();
        }
    }
}
