using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.LogViewer.Enums;
using QoDL.Toolkit.Core.Modules.LogViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Util
{
    internal class LogSearcher
    {
        private LogSearcherOptions Options { get; set; }

        public LogSearcher(LogSearcherOptions options)
        {
            Options = options;
        }

        public class InternalLogSearchResult
        {
            public List<LogEntry> MatchingEntries { get; set; } = new List<LogEntry>();
            public Dictionary<string, List<LogEntry>> GroupedEntries { get; set; } = new Dictionary<string, List<LogEntry>>();
            public int TotalMatchCount { get; set; }
            public DateTimeOffset? LowestDate { get; set; }
            public DateTimeOffset? HighestDate { get; set; }
            public List<LogSearchStatisticsResult> Statistics { get; set; } = new List<LogSearchStatisticsResult>();
            public bool StatisticsIsComplete => Statistics.Count == TotalMatchCount;
        }
        public InternalLogSearchResult SearchEntries(LogSearchFilter filter, CancellationToken cancellationToken, out int totalMatchCount)
        {
            var parsedQuery = QueryParser.ParseQuery(filter.Query, filter.QueryIsRegex);
            var parsedExcludedQuery = QueryParser.ParseQuery(filter.ExcludedQuery, filter.ExcludedQueryIsRegex);
            var parsedLogPathQuery = QueryParser.ParseQuery(filter.LogPathQuery, filter.LogPathQueryIsRegex);
            var parsedExcludedLogPathQuery = QueryParser.ParseQuery(filter.ExcludedLogPathQuery, filter.ExcludedLogPathQueryIsRegex);

            var logs = FindLogs();
            var entriesWithinDateThreshold = logs
                .WithCancellation(cancellationToken)
                .SelectMany(x => x.GetEntriesEnumerable(Options.EntryParser, filter.FromDate, filter.ToDate,
                    allowFilePath: (path) =>
                        parsedLogPathQuery.AllowItem(path, negate: false) && parsedExcludedLogPathQuery.AllowItem(path, negate: true)
                ))
                .Where(x =>
                    (filter.FromDate == null || x.Timestamp.ToUniversalTime() >= filter.FromDate?.ToUniversalTime())
                     && (filter.ToDate == null || x.Timestamp.ToUniversalTime() <= filter.ToDate?.ToUniversalTime())
                );

            var entries = entriesWithinDateThreshold
                .OrderByWithDirection(x => x.Timestamp, filter.OrderDescending)
                .Select(x => Options.EntryParser.ParseDetails(x))
                .AsEnumerable();

            entries = entries.Where(x => x.IsMargin || parsedQuery.AllowItem(x.Raw, negate: false));
            entries = entries.Where(x => x.IsMargin || parsedExcludedQuery.AllowItem(x.Raw, negate: true));

            var matchingEntries = entries.ToList();
            var statistics = matchingEntries
                .Take(filter.MaxStatisticsCount)
                .Select(x => new LogSearchStatisticsResult
                {
                    Timestamp = x.Timestamp,
                    Severity = ParseEntrySeverity(x.Raw)
                })
                .ToList();

            if (filter.MarginMilliseconds > 0)
            {
                matchingEntries = IncludeNeighbourEntries(matchingEntries, entriesWithinDateThreshold, filter.MarginMilliseconds)
                    .OrderByWithDirection(x => x.Timestamp, filter.OrderDescending)
                    .ToList();
            }

            totalMatchCount = matchingEntries.Count;

            var firstDate = matchingEntries.FirstOrDefault()?.Timestamp;
            var lastDate = matchingEntries.LastOrDefault()?.Timestamp;
            var highestDate = filter.OrderDescending ? firstDate : lastDate;
            var lowestDate = filter.OrderDescending ? lastDate : firstDate;

            Dictionary<string, List<LogEntry>> groupedEntries = new();
            if (parsedQuery.IsRegex && parsedQuery.RegexPattern.Contains("(?<GroupBy>"))
            {
                groupedEntries = matchingEntries
                    .GroupBy(x => parsedQuery.Regex.Match(x.Raw).Groups["GroupBy"].Value)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }

            return new InternalLogSearchResult
            {
                HighestDate = highestDate,
                LowestDate = lowestDate,
                TotalMatchCount = totalMatchCount,
                Statistics = statistics,
                MatchingEntries = matchingEntries
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .ToList(),
                GroupedEntries = groupedEntries
            };
        }


        private List<LogEntry> IncludeNeighbourEntries(List<LogEntry> matches, IEnumerable<LogEntry> allEntries, int marginMilliseconds)
        {
            var matchingDates = matches.Select(x => x.Timestamp);
            var dateRanges = GetTimeRanges(matchingDates, marginMilliseconds);

            return allEntries
                .Where(x =>
                    !matches.Any(m => m.FilePath == x.FilePath && m.LineNumber == x.LineNumber)
                    && dateRanges.Any(r => x.Timestamp.ToUniversalTime() >= r[0].ToUniversalTime() && x.Timestamp.ToUniversalTime() <= r[1].ToUniversalTime()))
                .Select(x =>
                {
                    x.IsMargin = true;
                    Options.EntryParser.ParseDetails(x);
                    return x;
                })
                .Union(matches)
                .ToList();
        }

        /// <summary>
        /// Creates a list of date ranges [date - margin, date + margin]
        /// </summary>
        private List<DateTimeOffset[]> GetTimeRanges(IEnumerable<DateTimeOffset> dates, int marginMilliseconds)
        {
            var dateRanges = new List<DateTimeOffset[]>();
            foreach (var date in dates)
            {
                var existingRange = dateRanges.FirstOrDefault(x => x[0] <= date && date <= x[1]);
                var low = date.AddMilliseconds(-marginMilliseconds);
                var high = date.AddMilliseconds(marginMilliseconds);
                if (existingRange != null)
                {
                    existingRange[0] = (existingRange[0].ToUniversalTime() > low.ToUniversalTime()) ? low : existingRange[0];
                    existingRange[1] = (existingRange[1].ToUniversalTime() < high.ToUniversalTime()) ? high : existingRange[1];
                    continue;
                }

                dateRanges.Add(new[] { low, high });
            }

            return dateRanges;
        }

        private static readonly Regex[] EntryErrorNeedles = new[]
        {
            new Regex(@"exception", RegexOptions.IgnoreCase),
            new Regex(@"[^""]error", RegexOptions.IgnoreCase),
            new Regex(@"\serr\s", RegexOptions.IgnoreCase),
            new Regex(@"critical", RegexOptions.IgnoreCase),
            new Regex(@"fatal", RegexOptions.IgnoreCase)
        };
        private static readonly Regex[] EntryWarningNeedles = new[]
        {
            new Regex(@"warning", RegexOptions.IgnoreCase),
            new Regex(@"\swarn\s", RegexOptions.IgnoreCase)
        };
        public static LogEntrySeverity ParseEntrySeverity(string rawEntry)
        {
            var normalizedContent = rawEntry.ToLower().Replace("\t", " ");

            if (EntryErrorNeedles.Any(x => x.IsMatch(normalizedContent)))
            {
                return LogEntrySeverity.Error;
            }
            else if (EntryWarningNeedles.Any(x => x.IsMatch(normalizedContent)))
            {
                return LogEntrySeverity.Warning;
            }

            return LogEntrySeverity.Info;
        }

        private List<LogFileGroup> FindLogs()
        {
            return GetLogFilepaths()
                .Select(x => LogFileGroup.FromFiles(Options.EntryParser, x))
                .Where(x => x.Files.Any())
                .OrderByDescending(x => x.LastFileWriteTime)
                .ToList();
        }

        private List<string> GetLogFilepaths()
        {
            return Options.LogFolders
                .SelectMany(x => Directory.GetFiles(x.Directory, x.FileFilter, x.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                .Union(Options.LogFilepaths)
                .ToList();
        }
    }

#if DEBUG
    // For LinqPad use :-)
#pragma warning disable CS1591
    public class LogSearcherExt
    {
        public object SearchEntries(string dir, LogSearchFilter filter, CancellationToken cancellationToken, out int totalMatchCount)
        {
            return new LogSearcher(new LogSearcherOptions(new LogEntryParser()).IncludeLogFilesInDirectory(dir))
                .SearchEntries(filter, cancellationToken, out totalMatchCount)
                .MatchingEntries.Select(x => new { File = x.FilePath, Line = x.LineNumber, Raw = x.Raw })
                .ToList();
        }
    }
#pragma warning restore CS1591
#endif
}