using QoDL.Toolkit.Core.Modules.Documentation.Attributes;
using QoDL.Toolkit.Core.Modules.Documentation.Models.FlowCharts;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Abstractions;

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