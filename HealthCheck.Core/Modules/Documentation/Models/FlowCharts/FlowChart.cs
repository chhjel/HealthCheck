using HealthCheck.Core.Modules.Documentation.Abstractions;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Documentation.Models.FlowCharts
{
    /// <summary>
    /// Data object created from <see cref="IFlowChartsService"/>
    /// </summary>
    public class FlowChart
    {
        /// <summary>
        /// Name of the diagram.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All the steps within the diagram.
        /// </summary>
        public List<FlowChartStep> Steps { get; set; } = new List<FlowChartStep>();
    }
}
