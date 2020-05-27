using HealthCheck.Core.Modules.AuditLog.Abstractions;

namespace HealthCheck.Core.Modules.AuditLog
{
    /// <summary>
    /// Options for <see cref="HCAuditLogModule"/>.
    /// </summary>
    public class HCAuditLogModuleOptions
    {
        /// <summary>
        /// Must be set for any site audits to be logged.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }
    }
}
