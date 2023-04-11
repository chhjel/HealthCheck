using System;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models;

/// <summary>
/// 
/// </summary>
public class TKJobStatus
{
    /// <summary></summary>
    internal string SourceId { get; set; }

    /// <summary></summary>
    public string JobId { get; set; }

    /// <summary></summary>
    public string Summary { get; set; }

    /// <summary></summary>
    public bool IsRunning { get; set; }

    /// <summary></summary>
    public bool IsEnabled { get; set; }

    /// <summary></summary>
    public DateTimeOffset? NextExecutionScheduledAt { get; set; }

    /// <summary></summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary></summary>
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary></summary>
    public TKJobHistoryStatus? Status { get; set; }
}
