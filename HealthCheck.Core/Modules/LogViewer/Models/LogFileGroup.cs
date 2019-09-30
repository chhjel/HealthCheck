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
        public DateTime LastFileWriteTime => Files.Select(x => x.LastWriteTime).Max();
        public List<LogFile> Files { get; set; }

        public static LogFileGroup FromDirectory(string directory)
        {
            return new LogFileGroup
            {
                Name = Path.GetFileName(directory),
                Files = Directory.GetFiles(directory)
                    .Select(file => new LogFile(file))
                    .OrderByDescending(file => file.LastWriteTime)
                    .ToList()
            };
        }

        public static LogFileGroup FromFiles(params string[] files)
        {
            return new LogFileGroup
            {
                Name = Path.GetFileName(files.First()),
                Files = files
                    .Select(file => new LogFile(file))
                    .OrderByDescending(file => file.LastWriteTime)
                    .ToList()
            };
        }

        public IEnumerable<string> GetLinesEnumerable(ILogEntryParser entryParser)
            => Files.Select(x => x.GetEnumerable(entryParser)).SelectMany(x => x);

        public IEnumerable<LogEntry> GetEntriesEnumerable(ILogEntryParser entryParser,
            DateTime? fromFileDate = null, DateTime? toFileDate = null,
            Func<string, LogSearchFilter, bool> allowFilePath = null, LogSearchFilter searchFilter = null
        )
            => Files
                .Where(x =>
                    allowFilePath(x.FilePath, searchFilter)
                    && (fromFileDate == null || x.LastWriteTime >= fromFileDate)
                    && (toFileDate == null || x.LastWriteTime <= toFileDate)
                )
                .Select(x => x.GetEntriesEnumerable(entryParser)).SelectMany(x => x);
    }
}