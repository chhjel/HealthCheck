using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Modules.LogViewer;
using HealthCheck.Core.Modules.LogViewer.Models;
using HealthCheck.Core.Services.Models;
using HealthCheck.Core.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static HealthCheck.Core.Modules.LogViewer.LogSearcher;

namespace HealthCheck.Core.Services
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
                Items = internalResult.MatchingEntries.Select(x => new LogEntrySearchResultItem()
                {
                    ColumnValues = x.ColumnValues,
                    FilePath = x.FilePath,
                    LineNumber = x.LineNumber,
                    IsMargin = x.IsMargin,
                    Raw = x.Raw,
                    Timestamp = x.Timestamp,
                    Severity = ParseEntrySeverity(x.Raw)
                }).ToList()
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
