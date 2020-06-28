using HealthCheck.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.LogViewer.Models
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
        /// Grouped results.
        /// </summary>
        public Dictionary<string, List<LogEntrySearchResultItem>> GroupedEntries { get; set; } = new Dictionary<string, List<LogEntrySearchResultItem>>();

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
        public DateTimeOffset? LowestDate { get; set; }

        /// <summary>
        /// Highest date amongst all matches.
        /// </summary>
        public DateTimeOffset? HighestDate { get; set; }

        /// <summary>
        /// First 5000 dates from matching entries.
        /// </summary>
        public List<LogSearchStatisticsResult> Statistics { get; set; } = new List<LogSearchStatisticsResult>();

        /// <summary>
        /// True if <see cref="Statistics"/> contains all the dates and not just the 5000 first.
        /// </summary>
        public bool StatisticsIsComplete => Statistics.Count == TotalCount;

        /// <summary>
        /// Parsed query.
        /// </summary>
        public ParsedQuery ParsedQuery { get; set; } = new ParsedQuery();

        /// <summary>
        /// Parsed excluded query.
        /// </summary>
        public ParsedQuery ParsedExcludedQuery { get; set; } = new ParsedQuery();

        /// <summary>
        /// Parsed log path query.
        /// </summary>
        public ParsedQuery ParsedLogPathQuery { get; set; } = new ParsedQuery();

        /// <summary>
        /// Parsed excluded log path query.
        /// </summary>
        public ParsedQuery ParsedExcludedLogPathQuery { get; set; } = new ParsedQuery();
    }
}
