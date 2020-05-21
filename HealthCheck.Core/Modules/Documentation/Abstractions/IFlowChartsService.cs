using HealthCheck.Core.Modules.Documentation.Attributes;
using HealthCheck.Core.Modules.Documentation.Models.FlowCharts;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.Documentation.Abstractions
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