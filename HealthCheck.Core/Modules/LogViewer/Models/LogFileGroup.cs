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
        public DateTime FirstEntryTime => Files.Select(x => x.FirstEntryTime).Min();
        public DateTime LastFileWriteTime => Files.Select(x => x.LastWriteTime).Max();
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
            DateTime? fromDate = null, DateTime? toDate = null,
            Func<string, LogSearchFilter, bool> allowFilePath = null, LogSearchFilter searchFilter = null
        )
            => Files
                .Where(x =>
                    allowFilePath(x.FilePath, searchFilter)
                    && (toDate == null   || toDate   >= x.FirstEntryTime)
                    && (fromDate == null || fromDate <= x.LastWriteTime)
                )
                .Select(x => x.GetEntriesEnumerable(entryParser)).SelectMany(x => x);
    }
}