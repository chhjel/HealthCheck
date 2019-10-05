using HealthCheck.Core.Modules.LogViewer.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HealthCheck.Core.Modules.LogViewer.Models
{
    internal class LogFile
    {
        public string FilePath { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime FirstEntryTime { get; set; }

        public LogFile(string path, ILogEntryParser entryParser)
        {
            FilePath = path;
            LastWriteTime = File.GetLastWriteTime(FilePath);

            var firstLine = File.ReadAllLines(FilePath).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            FirstEntryTime = entryParser.ParseEntryDate(firstLine) ?? DateTime.MaxValue;
        }

        public IEnumerable<LogEntry> GetEntriesEnumerable(ILogEntryParser entryParser)
        {
            return GetEnumerable(entryParser)
                .Select(x =>
                {
                    var delimiterIndex = x.IndexOf("|");
                    var lineNumber = x.Substring(0, delimiterIndex);
                    var rawEntry = x.Substring(delimiterIndex + 1);

                    var entryDate = entryParser.ParseEntryDate(rawEntry);
                    if (entryDate == null)
                    {
                        return null;
                    }

                    return new LogEntry()
                    {
                        FilePath = FilePath,
                        LineNumber = long.Parse(lineNumber),
                        Timestamp = entryDate.Value,
                        Raw = rawEntry
                    };
                })
                .Where(x => x != null);
        }

        public IEnumerable<string> GetEnumerable(ILogEntryParser entryParser)
        {
            long builderLines = 0;
            long lineNumber = 0;
            var stringbuilder = new StringBuilder();
            using (FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bufferedStream = new BufferedStream(fileStream))
            using (StreamReader streamReader = new StreamReader(bufferedStream))
            {
                string line;
                string nextStartingLine = null;
                var emptyLineCount = 0;
                var isFirstMatch = true;
                long firstMatchStart = 0;

                while ((line = streamReader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        emptyLineCount++;
                        continue;
                    }

                    stringbuilder.Clear();
                    builderLines = emptyLineCount;
                    emptyLineCount = 0;

                    var lineIsEntryStart = entryParser.IsEntryStart(line);
                    if (nextStartingLine != null)
                    {
                        stringbuilder.AppendLine(nextStartingLine);
                        builderLines++;
                        nextStartingLine = null;

                        if (!lineIsEntryStart)
                        {
                            stringbuilder.AppendLine(line);
                            builderLines++;
                        }
                    }

                    if (lineIsEntryStart)
                    {
                        if (isFirstMatch)
                        {
                            firstMatchStart = lineNumber;
                        }
                        if (stringbuilder.Length > 0)
                        {
                            yield return $"{lineNumber - builderLines}|{stringbuilder.ToString()}";
                            stringbuilder.Clear();
                            builderLines = 0;
                        }

                        stringbuilder.AppendLine(line);
                        builderLines++;
                    }
                    else if (streamReader.EndOfStream)
                    {
                        stringbuilder.AppendLine(line);
                        builderLines++;
                    }

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineNumber++;

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            builderLines++;
                            continue;
                        }

                        if (!entryParser.IsEntryStart(line))
                        {
                            nextStartingLine = null;
                            stringbuilder.AppendLine(line);
                            builderLines++;
                        }
                        else
                        {
                            nextStartingLine = line;
                            if (isFirstMatch)
                            {
                                isFirstMatch = false;
                                yield return $"{firstMatchStart}|{stringbuilder.ToString()}";
                            } else
                            {
                                yield return $"{lineNumber - builderLines}|{stringbuilder.ToString()}";
                            }
                            break;
                        }
                    }
                }

                if (nextStartingLine != null)
                {
                    yield return $"{lineNumber - builderLines + 1}|{nextStartingLine}";
                }
                else
                {
                    yield return $"{lineNumber - builderLines + 1}|{stringbuilder.ToString()}";
                }
            }
        }
    }
}