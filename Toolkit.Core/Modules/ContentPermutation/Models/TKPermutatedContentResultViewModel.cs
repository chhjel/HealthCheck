using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models;

/// <summary></summary>
public class TKPermutatedContentResultViewModel
{
    /// <summary></summary>
    public bool WasCached { get; set; }

    /// <summary></summary>
    public List<TKPermutatedContentItemViewModel> Content { get; set; }
}
