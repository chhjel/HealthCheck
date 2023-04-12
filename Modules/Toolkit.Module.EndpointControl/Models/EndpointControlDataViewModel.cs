using System.Collections.Generic;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary>
/// Contains initially loaded data on endpoint control page.
/// </summary>
public class EndpointControlDataViewModel
{
    /// <summary>
    /// All stored rules.
    /// </summary>
    public IEnumerable<EndpointControlRule> Rules { get; set; }

    /// <summary>
    /// All stored endpoint definitions.
    /// </summary>
    public IEnumerable<EndpointControlEndpointDefinition> EndpointDefinitions { get; set; }

    /// <summary>
    /// Any custom result definitions.
    /// </summary>
    public IEnumerable<EndpointControlCustomResultDefinitionViewModel> CustomResultDefinitions { get; set; }

    /// <summary>
    /// Any conditions.
    /// </summary>
    public IEnumerable<TKEndpointControlConditionDefinitionViewModel> Conditions { get; set; }
}
