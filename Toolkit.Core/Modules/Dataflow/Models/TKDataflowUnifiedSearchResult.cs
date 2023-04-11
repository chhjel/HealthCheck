using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Models;

/// <summary>
/// Result object from unified search.
/// </summary>
public class TKDataflowUnifiedSearchResult
{
    /// <summary>
    /// Results per stream.
    /// </summary>
    public List<TKDataflowUnifiedSearchStreamResult> StreamResults { get; set; } = new();
}
