using HealthCheck.Core.Abstractions;

namespace HealthCheck.Core.Modules.Diagrams.FlowCharts
{
    /// <summary>
    /// Data object created from <see cref="IFlowChartsService"/>
    /// </summary>
    public class FlowChartConnection
    {
        /// <summary>
        /// Title of the target node.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Label on the connection.
        /// </summary>
        public string Label { get; set; }
    }
}
