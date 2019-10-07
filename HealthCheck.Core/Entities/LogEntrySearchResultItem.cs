using HealthCheck.Core.Abstractions;
using System;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// Result from <see cref="ILogSearcherService"/>.
    /// </summary>
    public class LogEntrySearchResultItem
    {
        /// <summary>
        /// Path/name of the source log.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Raw log-entry contents.
        /// </summary>
        public string Raw { get; set; }

        /// <summary>
        /// Line-number of the log-entry.
        /// </summary>
        public long LineNumber { get; set; }

        /// <summary>
        /// When this entry was logged.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Column values if filter data contained column regex/delimiter data.
        /// </summary>
        public string[] ColumnValues { get; set; }

        /// <summary>
        /// Is not a direct match but a result included from setting a timestamp margin.
        /// </summary>
        public bool IsMargin { get; set; }
    }
}
