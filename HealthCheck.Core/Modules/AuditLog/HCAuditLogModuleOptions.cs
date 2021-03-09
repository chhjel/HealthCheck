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

        /// <summary>
        /// If set to true, client ip and useragent will be included in all stored events and not just in selected ones.
        /// </summary>
        public bool IncludeClientConnectionDetailsInAllEvents { get; set; }
    }
}
