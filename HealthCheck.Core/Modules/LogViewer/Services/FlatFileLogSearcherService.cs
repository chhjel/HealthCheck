using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.LogViewer.Models;
using HealthCheck.Core.Modules.LogViewer.Util;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static HealthCheck.Core.Modules.LogViewer.Util.LogSearcher;

namespace HealthCheck.Core.Modules.LogViewer.Services
{
    /// <summary>
    /// Log searcher implementation that searches content in log-files on disk.
    /// </summary>
    public class FlatFileLogSearcherService : ILogSearcherService
    {
        private FlatFileLogSearcherServiceOptions Options { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="FlatFileLogSearcherService"/>.
        /// <para>Log searcher implementation that searches content in log-files on disk.</para>
        /// </summary>
        public FlatFileLogSearcherService(FlatFileLogSearcherServiceOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Search logs using the given filter.
        /// </summary>
        public async Task<LogSearchResult> PerformSearchAsync(LogSearchFilter filter, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));

            var watch = new Stopwatch();
            watch.Start();

            var internalResult = SearchInternal(filter, cancellationToken);
            var duration = watch.ElapsedMilliseconds;
            var page = (filter.Skip / filter.Take) + 1;
            var pageCount = internalResult.TotalMatchCount / filter.Take;
            if (internalResult.TotalMatchCount % filter.Take != 0) pageCount++;

            var parsedQuery = QueryParser.ParseQuery(filter.Query, filter.QueryIsRegex);
            var parsedExcludedQuery = QueryParser.ParseQuery(filter.ExcludedQuery, filter.ExcludedQueryIsRegex);
            var parsedLogPathQuery = QueryParser.ParseQuery(filter.LogPathQuery, filter.LogPathQueryIsRegex);
            var parsedExcludedLogPathQuery = QueryParser.ParseQuery(filter.ExcludedLogPathQuery, filter.ExcludedLogPathQueryIsRegex);

            static LogEntrySearchResultItem entryViewModelFactory(LogEntry entry)
            {
                return new LogEntrySearchResultItem()
                {
                    ColumnValues = entry.ColumnValues,
                    FilePath = entry.FilePath,
                    LineNumber = entry.LineNumber,
                    IsMargin = entry.IsMargin,
                    Raw = entry.Raw,
                    Timestamp = entry.Timestamp,
                    Severity = ParseEntrySeverity(entry.Raw)
                };
            };

            return new LogSearchResult()
            {
                DurationInMilliseconds = duration,
                Error = internalResult.Error,
                ColumnNames = internalResult.ColumnNames,
                Count = internalResult.MatchingEntries.Count,
                TotalCount = internalResult.TotalMatchCount,
                PageCount = pageCount,
                CurrentPage = page,
                WasCancelled = internalResult.WasCancelled,
                Statistics = internalResult.Statistics,
                HighestDate = internalResult.HighestDate,
                LowestDate = internalResult.LowestDate,
                Items = internalResult.MatchingEntries.Select(x => entryViewModelFactory(x)).ToList(),
                GroupedEntries = internalResult.GroupedEntries.Select(x => 
                    new KeyValuePair<string, List<LogEntrySearchResultItem>>(
                        x.Key,
                        x.Value.Select(e => entryViewModelFactory(e)).ToList()
                    )
                ).ToDictionary(x => x.Key, x => x.Value),
                ParsedQuery = parsedQuery,
                ParsedExcludedQuery = parsedExcludedQuery,
                ParsedLogPathQuery = parsedLogPathQuery,
                ParsedExcludedLogPathQuery = parsedExcludedLogPathQuery
            };
        }
        
        private LogEntrySearchResult SearchInternal(LogSearchFilter filter, CancellationToken cancellationToken)
        {
            var columnRegex = string.IsNullOrWhiteSpace(filter.ColumnRegexPattern)
                ? null
                : new Regex(filter.ColumnRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var columnNames = columnRegex?.GetGroupNames()
                .Skip(1)
                .Select(x => int.TryParse(x, out int groupNumber) ? $"Column {groupNumber}" : x)
                .ToList();

            var entryParser = new LogEntryParser(columnRegex, filter.ColumnDelimiter);
            var searcherOptions = new LogSearcherOptions(entryParser);

            var logFolders = Options.LogFolders.Where(x => Directory.Exists(x.Directory));
            foreach(var folder in logFolders)
            {
                searcherOptions.IncludeLogFilesInDirectory(folder.Directory, folder.FileFilter, folder.Recursive);
            }

            var logFiles = Options.LogFilepaths.Where(x => File.Exists(x));
            foreach (var file in logFiles)
            {
                searcherOptions.IncludeLogFiles(file);
            }

            var wasCancelled = false;
            int totalMatchCount = 0;
            string error = null;
            var internalSearchResult = new InternalLogSearchResult();
            try
            {
                var searcher = new LogSearcher(searcherOptions);
                internalSearchResult = searcher.SearchEntries(filter, cancellationToken, out totalMatchCount);
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
            }
            catch (Exception ex)
            {
                error = ExceptionUtils.GetFullExceptionDetails(ex);
            }

            return new LogEntrySearchResult
            {
                ColumnNames = columnNames,
                MatchingEntries = internalSearchResult.MatchingEntries,
                GroupedEntries = internalSearchResult.GroupedEntries,
                Statistics = internalSearchResult.Statistics,
                HighestDate = internalSearchResult.HighestDate,
                LowestDate = internalSearchResult.LowestDate,
                TotalMatchCount = totalMatchCount,
                WasCancelled = wasCancelled,
                Error = error
            };
        }

    }
}
