using System.Collections.Generic;

namespace QoDL.Toolkit.Module.EndpointControl.Models;

/// <summary></summary>
public class TKEndpointControlConditionData
{
    /// <summary></summary>
    public string ConditionId { get; set; }

    /// <summary></summary>
    public Dictionary<string, string> Parameters { get; set; }
}
