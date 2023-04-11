using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary></summary>
public class EndpointControlCustomResultDefinitionViewModel
{
    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary></summary>
    public List<TKBackendInputConfig> CustomProperties { get; set; }
}
