using HealthCheck.Core.Modules.AuditLog.Abstractions;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Services related to the HealthCheck namespace.
    /// </summary>
    public class HealthCheckServiceContainer<TAccessRole>
    {
        /// <summary>
        /// Todo: remove.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }
    }
}
