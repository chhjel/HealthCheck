using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models;

/// <summary></summary>
public class TKPagedJobHistoryEntry
{
    /// <summary></summary>
    public int TotalCount { get; set; }
    /// <summary></summary>
    public List<TKJobHistoryEntry> Items { get; set; }
}
