using HealthCheck.Core.Modules.Diagrams.FlowCharts;
using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s.
    /// </summary>
    public interface IFlowChartsService
    {
        /// <summary>
        /// Generates flow chart diagram data from <see cref="SequenceDiagramStepAttribute"/>s in the given assemblies.
        /// </summary>
        List<FlowChart> Generate(IEnumerable<Assembly> sourceAssemblies = null);
    }
}