using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Models;

internal class LogEntrySearchResult
{
    public List<string> ColumnNames { get; set; }
    public List<LogEntry> MatchingEntries { get; set; }
    public Dictionary<string, List<LogEntry>> GroupedEntries = new();
    public int TotalMatchCount { get; set; }
    public bool WasCancelled { get; set; }

    public string Error { get; set; }
    public bool HasError => !string.IsNullOrWhiteSpace(Error);

    public DateTimeOffset? LowestDate { get; set; }
    public DateTimeOffset? HighestDate { get; set; }
    public List<LogSearchStatisticsResult> Statistics { get; set; } = new List<LogSearchStatisticsResult>();
    public bool StatisticsIsComplete => Statistics.Count == TotalMatchCount;
}
