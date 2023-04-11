using System;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Models;

internal class LogEntry
{
    public string FilePath { get; set; }
    public string Raw { get; set; }

    public long LineNumber { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool IsMargin { get; set; }
    public string[] ColumnValues { get; set; }
}