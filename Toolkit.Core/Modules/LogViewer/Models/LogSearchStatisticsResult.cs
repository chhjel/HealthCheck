using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.LogViewer.Enums;
using System;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Models;

/// <summary>
/// Statistics result item from <see cref="ILogSearcherService"/>.
/// </summary>
public class LogSearchStatisticsResult
{
    /// <summary>
    /// Timestamp of the log entry.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Parsed severity of the entry.
    /// </summary>
    public LogEntrySeverity Severity { get; set; }
}
