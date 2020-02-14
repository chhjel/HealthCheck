using HealthCheck.Core.Abstractions;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Services related to the HealthCheck namespace.
    /// </summary>
    public class HealthCheckServiceContainer
    {
        /// <summary>
        /// Must be set for any site statuses to be stored and returned.
        /// </summary>
        public ISiteEventService SiteEventService { get; set; }

        /// <summary>
        /// Must be set for any site audits to be logged.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }

        /// <summary>
        /// Must be set for the log viewer tab to be displayed.
        /// </summary>
        public ILogSearcherService LogSearcherService { get; set; }

        /// <summary>
        /// Must be set for the requestlog to be displayed.
        /// </summary>
        public IRequestLogService RequestLogService { get; set; }

        /// <summary>
        /// Must be set for the documentation tab to be shown.
        /// </summary>
        public ISequenceDiagramService SequenceDiagramService { get; set; }
    }
}
