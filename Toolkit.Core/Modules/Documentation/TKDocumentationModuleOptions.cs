using QoDL.Toolkit.Core.Modules.Documentation.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Documentation;

/// <summary>
/// Options for <see cref="TKDocumentationModule"/>.
/// </summary>
public class TKDocumentationModuleOptions
{
    /// <summary>
    /// Handles sequence diagrams.
    /// </summary>
    public ISequenceDiagramService SequenceDiagramService { get; set; }

    /// <summary>
    /// Handles flow charts.
    /// </summary>
    public IFlowChartsService FlowChartsService { get; set; }

    /// <summary>
    /// If true then diagram data will only be retrieved from services once and cached in memory.
    /// </summary>
    public bool CacheDiagrams { get; set; } = true;

    /// <summary>
    /// Show a diagram sandbox.
    /// <para>Defaults to false.</para>
    /// </summary>
    public bool EnableDiagramSandbox { get; set; }

    /// <summary>
    /// Show extra diagram details.
    /// <para>For sequence diagrams class/method names will be shown on step clicks.</para>
    /// <para>Defaults to false.</para>
    /// </summary>
    public bool EnableDiagramDetails { get; set; }
}
