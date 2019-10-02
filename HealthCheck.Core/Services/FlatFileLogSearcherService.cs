﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Modules.LogViewer;
using HealthCheck.Core.Modules.LogViewer.Models;
using HealthCheck.Core.Services.Models;

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

            return new LogSearchResult()
            {
                DurationInMilliseconds = duration,
                ColumnNames = internalResult.ColumnNames,
                Count = internalResult.MatchingEntries.Count,
                TotalCount = internalResult.TotalMatchCount,
                WasCancelled = internalResult.WasCancelled,
                Items = internalResult.MatchingEntries.Select(x => new LogEntrySearchResultItem()
                {
                    ColumnValues = x.ColumnValues,
                    FilePath = x.FilePath,
                    LineNumber = x.LineNumber,
                    Raw = x.Raw,
                    Timestamp = x.Timestamp
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
            List<LogEntry> matchingEntries = new List<LogEntry>();

            try
            {
                var searcher = new LogSearcher(searcherOptions);
                matchingEntries = searcher.SearchEntries(filter, out totalMatchCount, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
            }

            return new LogEntrySearchResult
            {
                ColumnNames = columnNames,
                MatchingEntries = matchingEntries,
                TotalMatchCount = totalMatchCount,
                WasCancelled = wasCancelled
            };
        }

    }
}
