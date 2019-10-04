using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    internal class LogEntrySearchResult
    {
        public List<string> ColumnNames { get; set; }
        public List<LogEntry> MatchingEntries { get; set; }
        public int TotalMatchCount { get; set; }
        public bool WasCancelled { get; set; }

        public string Error { get; set; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public DateTime? LowestDate { get; set; }
        public DateTime? HighestDate { get; set; }
        public List<DateTime> Dates { get; set; } = new List<DateTime>();
        public bool AllDatesIncluded => Dates.Count == TotalMatchCount;
    }
}
