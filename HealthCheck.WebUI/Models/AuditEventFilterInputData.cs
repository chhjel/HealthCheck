using System;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Input data type for the audit event filter endpoint.
    /// </summary>
    public class AuditEventFilterInputData
    {
        /// <summary>
        /// Subject filter.
        /// </summary>
        public string SubjectFilter { get; set; }

        /// <summary>
        /// Title filter.
        /// </summary>
        public string TitleFilter { get; set; }

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
