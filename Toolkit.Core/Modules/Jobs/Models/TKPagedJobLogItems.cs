using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models;

/// <summary></summary>
public class TKPagedJobLogItems
{
    /// <summary></summary>
    public long TotalCount { get; set; }

    /// <summary></summary>
    public List<TKJobLogItem> Items { get; set; } = new();
}
