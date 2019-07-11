using HealthCheck.Core.Enums;
using System;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Input data type for the audit event filter endpoint.
    /// </summary>
    public class AuditEventFilterInputData
    {
        /// <summary>
        /// Filter on area.
        /// </summary>
        public AuditEventArea? AreaFilter { get; set; }

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
        public DateTime? FromFilter { get; set; }

        /// <summary>
        /// Date to filter.
        /// </summary>
        public DateTime? ToFilter { get; set; }
    }
}
