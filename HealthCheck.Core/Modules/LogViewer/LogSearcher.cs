using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.LogViewer.Enums;
using HealthCheck.Core.Modules.LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace HealthCheck.Core.Modules.LogViewer
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
            public int TotalMatchCount { get; set; }
            public DateTime? LowestDate { get; set; }
            public DateTime? HighestDate { get; set; }
            public List<DateTime> Dates { get; set; } = new List<DateTime>();
            public bool AllDatesIncluded => Dates.Count == TotalMatchCount;
        }
        public InternalLogSearchResult SearchEntries(LogSearchFilter filter, CancellationToken cancellationToken, out int totalMatchCount)
        {
            var logs = FindLogs();
            var entriesWithinDateThreshold = logs
                .WithCancellation(cancellationToken)
                .SelectMany(x => x.GetEntriesEnumerable(Options.EntryParser, filter.FromDate, filter.ToDate, AllowLogFile, filter))
                .Where(x =>
                    (filter.FromDate == null || x.Timestamp >= filter.FromDate)
                     && (filter.ToDate == null || x.Timestamp <= filter.ToDate)
                );

            var entries = entriesWithinDateThreshold
                .OrderByWithDirection(x => x.Timestamp, filter.OrderDescending)
                .Select(x => Options.EntryParser.ParseDetails(x))
                .AsEnumerable();

            entries = ProcessQueryFilter(entries, filter.Query, filter.QueryMode, negate: false);
            entries = ProcessQueryFilter(entries, filter.ExcludedQuery, filter.ExcludedQueryMode, negate: true);

            var matchingEntries = entries.ToList();
            var dates = matchingEntries.Select(x => x.Timestamp).Take(filter.MaxDateCount).ToList();

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

            return new InternalLogSearchResult
            {
                HighestDate = highestDate,
                LowestDate = lowestDate,
                TotalMatchCount = totalMatchCount,
                Dates = dates,
                MatchingEntries = matchingEntries
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .ToList()
            };
        }

        private List<LogEntry> IncludeNeighbourEntries(List<LogEntry> matches, IEnumerable<LogEntry> allEntries, int marginMilliseconds)
        {
            var matchingDates = matches.Select(x => x.Timestamp);
            var dateRanges = GetTimeRanges(matchingDates, marginMilliseconds);

            return allEntries
                .Where(x =>
                    !matches.Any(m => m.FilePath == x.FilePath && m.LineNumber == x.LineNumber)
                    && dateRanges.Any(r => x.Timestamp >= r[0] && x.Timestamp <= r[1]))
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
        private List<DateTime[]> GetTimeRanges(IEnumerable<DateTime> dates, int marginMilliseconds)
        {
            var dateRanges = new List<DateTime[]>();
            foreach (var date in dates)
            {
                var existingRange = dateRanges.FirstOrDefault(x => x[0] <= date && date <= x[1]);
                var low = date.AddMilliseconds(-marginMilliseconds);
                var high = date.AddMilliseconds(marginMilliseconds);
                if (existingRange != null)
                {
                    existingRange[0] = (existingRange[0] > low) ? low : existingRange[0];
                    existingRange[1] = (existingRange[1] < high) ? high : existingRange[1];
                    continue;
                }

                dateRanges.Add(new[] { low, high });
            }

            return dateRanges;
        }

        private List<LogFileGroup> FindLogs()
        {
            return GetLogFilepaths()
                // .GroupBy(path => path.Substring(0, path.IndexOf(".")))
                // .Select(group => LogFileGroup.FromFiles(group))
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

        private bool AllowLogFile(string logFilePath, LogSearchFilter filter)
        {
            return AllowLogFilePath(logFilePath, filter.LogPathQuery, filter.LogPathQueryMode, negate: false)
                && AllowLogFilePath(logFilePath, filter.ExcludedLogPathQuery, filter.ExcludedLogPathQueryMode, negate: true);
        }

        private bool AllowLogFilePath(string logFilePath, string query, FilterQueryMode mode, bool negate)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            Func<string, bool> queryPredicate = CreateQueryPredicate(query, mode, negate);
            return (queryPredicate != null) ? queryPredicate(logFilePath) : true;
        }

        private IEnumerable<LogEntry> ProcessQueryFilter(IEnumerable<LogEntry> entries, string query, FilterQueryMode mode, bool negate)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return entries;
            }

            Func<string, bool> queryPredicate = CreateQueryPredicate(query, mode, negate);
            if (queryPredicate != null)
            {
                entries = entries.Where(entry => !entry.IsMargin && queryPredicate(entry.Raw));
            }

            return entries;
        }

        private readonly Dictionary<string, Regex> RegexCache = new Dictionary<string, Regex>();
        private Func<string, bool> CreateQueryPredicate(string query, FilterQueryMode mode, bool negate)
        {
            Func<string, bool> predicate = null;
            if (mode == FilterQueryMode.Exact)
            {
                predicate = raw => raw.ToLower().Contains(query.ToLower());
            }
            else if (mode == FilterQueryMode.AnyWord)
            {
                var queryWords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower());
                predicate = raw => queryWords.Any(word => raw.ToLower().Contains(word));
            }
            else if (mode == FilterQueryMode.AllWords)
            {
                var queryWords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower());
                predicate = raw => queryWords.All(word => raw.ToLower().Contains(word));
            }
            else if (mode == FilterQueryMode.Regex)
            {
                Regex regex = null;
                if (RegexCache.ContainsKey(query))
                {
                    regex = RegexCache[query];
                }
                else
                {
                    regex = new Regex(query, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    RegexCache[query] = regex;
                }

                predicate = raw => regex.IsMatch(raw);
            }

            return negate ? (raw) => !predicate(raw) : predicate;
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