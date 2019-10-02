using System.Collections.Generic;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    internal class LogEntrySearchResult
    {
        public List<string> ColumnNames { get; set; }
        public List<LogEntry> MatchingEntries { get; set; }
        public int TotalMatchCount { get; set; }
        public bool WasCancelled { get; set; }
    }
}