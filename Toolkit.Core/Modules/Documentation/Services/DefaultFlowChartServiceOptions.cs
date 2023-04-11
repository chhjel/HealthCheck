using QoDL.Toolkit.Core.Modules.Documentation.Abstractions;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Services;

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
