using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKDataRepeaterStreamBatchActionViewModel
{
    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary></summary>
    public string ExecuteButtonLabel { get; set; }

    /// <summary></summary>
    public List<TKBackendInputConfig> ParameterDefinitions { get; set; } = new();
}
