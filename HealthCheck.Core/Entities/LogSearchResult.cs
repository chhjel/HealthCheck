using HealthCheck.Core.Abstractions;
using System;
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
        /// Number of pages in total.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Current page the results are on.
        /// </summary>
        public int CurrentPage { get; set; }

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

        /// <summary>
        /// Any exception that occured.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// True if <see cref="Error"/> is not empty.
        /// </summary>
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        /// <summary>
        /// Lowest date amongst all matches.
        /// </summary>
        public DateTime? LowestDate { get; set; }

        /// <summary>
        /// Highest date amongst all matches.
        /// </summary>
        public DateTime? HighestDate { get; set; }

        /// <summary>
        /// First 1000 dates from matching entries.
        /// </summary>
        public List<DateTime> Dates { get; set; } = new List<DateTime>();

        /// <summary>
        /// True if <see cref="Dates"/> contains all the dates and not just the 1000 first.
        /// </summary>
        public bool AllDatesIncluded => Dates.Count == TotalCount;
    }
}
