using System;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    internal class LogEntry
    {
        public string FilePath { get; set; }
        public string Raw { get; set; }

        public long LineNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsMargin { get; set; }
        public string[] ColumnValues { get; set; }
    }
}