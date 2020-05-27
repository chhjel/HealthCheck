using HealthCheck.Core.Modules.Documentation.Abstractions;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Documentation.Models.FlowCharts
{
    /// <summary>
    /// Data object created from <see cref="IFlowChartsService"/>
    /// </summary>
    public class FlowChartStep
    {
        /// <summary>
        /// Title of the node.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Element type to display.
        /// <para>Defaults to selecting type based on title and connections.</para>
        /// </summary>
        public FlowChartStepType? Type { get; set; }

        /// <summary>
        /// All the steps within the diagram.
        /// </summary>
        public List<FlowChartConnection> Connections { get; set; } = new List<FlowChartConnection>();

        /// <summary>
        /// Name of the class the attribute was placed on.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Name of the method the attribute was placed on.
        /// </summary>
        public string MethodName { get; set; }
    }
}
