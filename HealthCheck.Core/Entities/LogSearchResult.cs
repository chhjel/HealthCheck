using HealthCheck.Core.Abstractions;
using System.Collections.Generic;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// Result from <see cref="ILogSearcherService"/>.
    /// </summary>
    public class LogSearchResult
    {
        /// <summary>
        /// Total result count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Result count only including returned results.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Returned results.
        /// </summary>
        public List<LogEntrySearchResultItem> Items { get; set; } = new List<LogEntrySearchResultItem>();

        /// <summary>
        /// Result column names.
        /// </summary>
        public List<string> ColumnNames { get; set; } = new List<string>();

        /// <summary>
        /// How long the search process lasted.
        /// </summary>
        public long DurationInMilliseconds { get; set; }

        /// <summary>
        /// True if the search was cancelled.
        /// </summary>
        public bool WasCancelled { get; set; }
    }
}
