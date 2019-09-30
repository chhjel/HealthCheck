using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.LogViewer.Enums;
using System;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    /// <summary>
    /// Filter data for <see cref="ILogSearcherService"/>.
    /// </summary>
    public class LogSearchFilter
    {
        /// <summary>
        /// Number of entries to skip.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Max number of entries to include.
        /// </summary>
        public int Take { get; set; } = 100;

        /// <summary>
        /// Lowest date to include.
        /// </summary>
        public DateTime? FromFileDate { get; set; }

        /// <summary>
        /// Highest date to include.
        /// </summary>
        public DateTime? ToFileDate { get; set; }

        /// <summary>
        /// Value that should be included in the logs content.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Filter mode for <see cref="Query"/>.
        /// </summary>
        public FilterQueryMode QueryMode { get; set; } = FilterQueryMode.Exact;

        /// <summary>
        /// Value that should not be included in the logs content.
        /// </summary>
        public string ExcludedQuery { get; set; }

        /// <summary>
        /// Filter mode for <see cref="ExcludedQuery"/>.
        /// </summary>
        public FilterQueryMode ExcludedQueryMode { get; set; } = FilterQueryMode.Exact;

        /// <summary>
        /// Value that should be included in the logs filepath or name.
        /// </summary>
        public string LogPathQuery { get; set; }

        /// <summary>
        /// Filter mode for <see cref="LogPathQuery"/>.
        /// </summary>
        public FilterQueryMode LogPathQueryMode { get; set; } = FilterQueryMode.Exact;

        /// <summary>
        /// Value that should not be included in the logs filepath or name.
        /// </summary>
        public string ExcludedLogPathQuery { get; set; }

        /// <summary>
        /// Filter mode for <see cref="ExcludedLogPathQuery"/>.
        /// </summary>
        public FilterQueryMode ExcludedLogPathQueryMode { get; set; } = FilterQueryMode.Exact;

        /// <summary>
        /// Optional regex pattern that columns should be created from.
        /// </summary>
        public string ColumnRegexPattern { get; set; }

        /// <summary>
        /// Optional delimiter to create columns from.
        /// </summary>
        public string ColumnDelimiter { get; set; }

        /// <summary>
        /// Order descending instead of ascending.
        /// </summary>
        public bool OrderDescending { get; set; }
    }
}