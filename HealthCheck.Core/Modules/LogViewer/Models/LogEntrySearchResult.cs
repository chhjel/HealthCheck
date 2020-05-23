using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    internal class LogEntrySearchResult
    {
        public List<string> ColumnNames { get; set; }
        public List<LogEntry> MatchingEntries { get; set; }
        public Dictionary<string, List<LogEntry>> GroupedEntries = new Dictionary<string, List<LogEntry>>();
        public int TotalMatchCount { get; set; }
        public bool WasCancelled { get; set; }

        public string Error { get; set; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public DateTime? LowestDate { get; set; }
        public DateTime? HighestDate { get; set; }
        public List<LogSearchStatisticsResult> Statistics { get; set; } = new List<LogSearchStatisticsResult>();
        public bool StatisticsIsComplete => Statistics.Count == TotalMatchCount;
    }
}
