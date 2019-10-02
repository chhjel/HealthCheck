using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.LogViewer.Enums;
using HealthCheck.Core.Modules.LogViewer.Models;
using System;
using System.Collections.Generic;
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

        public List<LogEntry> SearchEntries(LogSearchFilter filter, CancellationToken cancellationToken)
            => SearchEntries(filter, out int _, cancellationToken);

        public List<LogEntry> SearchEntries(LogSearchFilter filter, out int totalMatchCount, CancellationToken cancellationToken)
        {
            var logs = FindLogs();
            var entries = logs
                .WithCancellation(cancellationToken)
                .SelectMany(x => x.GetEntriesEnumerable(Options.EntryParser, filter.FromFileDate, filter.ToFileDate, AllowLogFile, filter))
                .Where(x =>
                    (filter.FromFileDate == null || x.Timestamp >= filter.FromFileDate)
                     && (filter.ToFileDate == null || x.Timestamp <= filter.ToFileDate)
                )
                .OrderByWithDirection(x => x.Timestamp, filter.OrderDescending)
                .Select(x => Options.EntryParser.ParseDetails(x))
                .AsEnumerable();

            entries = ProcessQueryFilter(entries, filter.Query, filter.QueryMode, negate: false);
            entries = ProcessQueryFilter(entries, filter.ExcludedQuery, filter.ExcludedQueryMode, negate: true);

            var matchingEntries = entries.ToList();
            totalMatchCount = matchingEntries.Count;

            return matchingEntries
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToList();
        }

        private List<LogFileGroup> FindLogs()
        {
            return GetLogFilepaths()
                // .GroupBy(path => path.Substring(0, path.IndexOf(".")))
                // .Select(group => LogFileGroup.FromFiles(group))
                .Select(x => LogFileGroup.FromFiles(x))
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
                entries = entries.Where(entry => queryPredicate(entry.Raw));
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
}