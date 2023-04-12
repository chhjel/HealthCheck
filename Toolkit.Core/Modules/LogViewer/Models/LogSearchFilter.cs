using QoDL.Toolkit.Core.Abstractions;
using System;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Models;

/// <summary>
/// Filter data for <see cref="ILogSearcherService"/>.
/// </summary>
public class LogSearchFilter
{
    /// <summary>
    /// Id of this search.
    /// </summary>
    public string SearchId { get; set; }

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
    public DateTimeOffset? FromDate { get; set; }

    /// <summary>
    /// Highest date to include.
    /// </summary>
    public DateTimeOffset? ToDate { get; set; }

    /// <summary>
    /// Value that should be included in the logs content.
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    /// If true then <see cref="Query"/> will be treated as RegExp.
    /// </summary>
    public bool QueryIsRegex { get; set; }

    /// <summary>
    /// Value that should not be included in the logs content.
    /// </summary>
    public string ExcludedQuery { get; set; }

    /// <summary>
    /// If true then <see cref="ExcludedQuery"/> will be treated as RegExp.
    /// </summary>
    public bool ExcludedQueryIsRegex { get; set; }

    /// <summary>
    /// Value that should be included in the logs filepath or name.
    /// </summary>
    public string LogPathQuery { get; set; }

    /// <summary>
    /// If true then <see cref="LogPathQuery"/> will be treated as RegExp.
    /// </summary>
    public bool LogPathQueryIsRegex { get; set; }

    /// <summary>
    /// Value that should not be included in the logs filepath or name.
    /// </summary>
    public string ExcludedLogPathQuery { get; set; }

    /// <summary>
    /// If true then <see cref="ExcludedLogPathQuery"/> will be treated as RegExp.
    /// </summary>
    public bool ExcludedLogPathQueryIsRegex { get; set; }

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

    /// <summary>
    /// Max number of dates to include.
    /// </summary>
    public int MaxStatisticsCount { get; set; }

    /// <summary>
    /// Optional number of milliseconds to include entries before/after any matches.
    /// </summary>
    public int MarginMilliseconds { get; set; }
}