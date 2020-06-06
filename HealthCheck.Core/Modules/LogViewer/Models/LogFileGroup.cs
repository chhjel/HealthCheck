using HealthCheck.Core.Modules.LogViewer.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    internal class LogFileGroup
    {
        public string Name { get; set; }
        public DateTimeOffset FirstEntryTime => Files.Select(x => x.FirstEntryTime).Min();
        public DateTimeOffset LastFileWriteTime => Files.Select(x => x.LastWriteTime).Max();
        public List<LogFile> Files { get; set; }

        public static LogFileGroup FromFiles(ILogEntryParser entryParser, params string[] files)
        {
            return new LogFileGroup
            {
                Name = Path.GetFileName(files.First()),
                Files = files
                    .Select(file => new LogFile(file, entryParser))
                    .OrderByDescending(file => file.LastWriteTime)
                    .ToList()
            };
        }

        public IEnumerable<string> GetLinesEnumerable(ILogEntryParser entryParser)
            => Files.Select(x => x.GetEnumerable(entryParser)).SelectMany(x => x);

        public IEnumerable<LogEntry> GetEntriesEnumerable(ILogEntryParser entryParser,
            DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null,
            Func<string, bool> allowFilePath = null
        )
        {
            DateTimeOffset? lastWriteLowThreshold = (fromDate == null) ? null : fromDate - TimeSpan.FromDays(1);
            DateTimeOffset? lastWriteHighThreshold = (toDate == null) ? null : toDate + TimeSpan.FromDays(1);

            return Files
                .Where(x =>
                    allowFilePath(x.FilePath)
                    && (lastWriteLowThreshold == null || x.LastWriteTime.ToUniversalTime() >= lastWriteLowThreshold?.ToUniversalTime())
                    && (lastWriteHighThreshold == null || x.LastWriteTime.ToUniversalTime() <= lastWriteHighThreshold?.ToUniversalTime())
                    && (toDate == null || toDate?.ToUniversalTime() >= x.FirstEntryTime.ToUniversalTime())
                    && (fromDate == null || fromDate?.ToUniversalTime() <= x.LastWriteTime.ToUniversalTime())
                )
                .Select(x => x.GetEntriesEnumerable(entryParser)).SelectMany(x => x);
        }
    }
}