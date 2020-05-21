using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.EventNotifications;

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

        /// <summary>
        /// Must be set for the log viewer tab to be displayed.
        /// </summary>
        public ILogSearcherService LogSearcherService { get; set; }

        /// <summary>
        /// Must be set for the event notifications tab to be shown.
        /// </summary>
        public IEventDataSink EventSink { get; set; }
    }
}
