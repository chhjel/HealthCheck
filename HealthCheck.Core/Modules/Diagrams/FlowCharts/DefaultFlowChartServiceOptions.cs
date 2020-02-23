using HealthCheck.Core.Abstractions;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.Diagrams.FlowCharts
{
    /// <summary>
    /// Options for <see cref="DefaultFlowChartService"/>
    /// </summary>
    public class DefaultFlowChartServiceOptions
    {
        /// <summary>
        /// Default assemblies to detect flow chart data from if no assemblies are specified in the 
        /// <see cref="IFlowChartsService.Generate(IEnumerable{Assembly})"/> method.
        /// </summary>
        public IEnumerable<Assembly> DefaultSourceAssemblies { get; set; }
    }
}
