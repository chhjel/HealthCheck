using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKGetDataRepeaterStreamDefinitionsViewModel
{
    /// <summary></summary>
    public List<TKDataRepeaterStreamViewModel> Streams { get; set; } = new();
}
