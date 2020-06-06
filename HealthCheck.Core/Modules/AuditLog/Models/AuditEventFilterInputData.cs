using System;

namespace HealthCheck.Core.Modules.AuditLog.Models
{
    /// <summary>
    /// Input data type for the audit event filter endpoint.
    /// </summary>
    public class AuditEventFilterInputData
    {
        /// <summary>
        /// Filter on area.
        /// </summary>
        public string AreaFilter { get; set; }

        /// <summary>
        /// Filter on action that was performed.
        /// </summary>
        public string ActionFilter { get; set; }

        /// <summary>
        /// Filter on the object that action was performed against.
        /// </summary>
        public string SubjectFilter { get; set; }

        /// <summary>
        /// User id filter.
        /// </summary>
        public string UserIdFilter { get; set; }

        /// <summary>
        /// Username filter.
        /// </summary>
        public string UserNameFilter { get; set; }

        /// <summary>
        /// Date from filter.
        /// </summary>
        public DateTimeOffset? FromFilter { get; set; }

        /// <summary>
        /// Date to filter.
        /// </summary>
        public DateTimeOffset? ToFilter { get; set; }
    }
}
