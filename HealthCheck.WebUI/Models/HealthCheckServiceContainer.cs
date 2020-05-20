using HealthCheck.Core.Abstractions;
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
        /// Must be set for any site audits to be logged.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }

        /// <summary>
        /// Must be set for the log viewer tab to be displayed.
        /// </summary>
        public ILogSearcherService LogSearcherService { get; set; }

        /// <summary>
        /// Either this or <see cref="FlowChartsService"/> must be set for the documentation tab to be shown.
        /// </summary>
        public ISequenceDiagramService SequenceDiagramService { get; set; }

        /// <summary>
        /// Either this or <see cref="SequenceDiagramService"/> must be set for the documentation tab to be shown.
        /// </summary>
        public IFlowChartsService FlowChartsService { get; set; }

        /// <summary>
        /// Must be set for the dataflow tab to be shown.
        /// </summary>
        public IDataflowService<TAccessRole> DataflowService { get; set; }

        /// <summary>
        /// Must be set for the event notifications tab to be shown.
        /// </summary>
        public IEventDataSink EventSink { get; set; }

        internal bool IsAnyDocumentationServiceSet => SequenceDiagramService != null || FlowChartsService != null;
    }
}
