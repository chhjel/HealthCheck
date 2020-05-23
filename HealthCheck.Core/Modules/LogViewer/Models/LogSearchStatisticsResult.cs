using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.LogViewer.Enums;
using System;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    /// <summary>
    /// Statistics result item from <see cref="ILogSearcherService"/>.
    /// </summary>
    public class LogSearchStatisticsResult
    {
        /// <summary>
        /// Timestamp of the log entry.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Parsed severity of the entry.
        /// </summary>
        public LogEntrySeverity Severity { get; set; }
    }
}
