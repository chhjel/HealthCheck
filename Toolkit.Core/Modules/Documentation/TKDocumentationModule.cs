using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.Documentation.Models;
using QoDL.Toolkit.Core.Modules.Documentation.Models.FlowCharts;
using QoDL.Toolkit.Core.Modules.Documentation.Models.SequenceDiagrams;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Documentation;

/// <summary>
/// Module for viewing documentation.
/// </summary>
public class TKDocumentationModule : ToolkitModuleBase<TKDocumentationModule.AccessOption>
{
    private TKDocumentationModuleOptions Options { get; }
    private static DiagramDataViewModel DiagramDataViewModelCache { get; set; }

    /// <summary>
    /// Module for viewing documentation.
    /// </summary>
    public TKDocumentationModule(TKDocumentationModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => new
    {
        EnableDiagramSandbox = Options.EnableDiagramSandbox,
        EnableDiagramDetails = Options.EnableDiagramDetails
    };

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKDocumentationModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,
    }

    #region Invokable methods
    /// <summary>
    /// Get diagrams to show in the UI.
    /// </summary>
    [ToolkitModuleMethod]
    public DiagramDataViewModel GetDiagrams()
    {
        if (Options.CacheDiagrams && DiagramDataViewModelCache != null)
        {
            return DiagramDataViewModelCache;
        }

        DiagramDataViewModelCache = new DiagramDataViewModel()
        {
            SequenceDiagrams = Options.SequenceDiagramService?.Generate() ?? new List<SequenceDiagram>(),
            FlowCharts = Options.FlowChartsService?.Generate() ?? new List<FlowChart>()
        };
        return DiagramDataViewModelCache;
    }
    #endregion
}
