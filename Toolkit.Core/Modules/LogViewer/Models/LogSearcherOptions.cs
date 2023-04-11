using QoDL.Toolkit.Core.Modules.LogViewer.Abstractions;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Models;

internal class LogSearcherOptions
{
    public List<string> LogFilepaths { get; set; } = new List<string>();
    public List<LogFolderSource> LogFolders { get; set; } = new List<LogFolderSource>();
    public ILogEntryParser EntryParser { get; set; }

    public LogSearcherOptions(ILogEntryParser entryParser)
    {
        EntryParser = entryParser;
    }

    public LogSearcherOptions IncludeLogFiles(params string[] filepaths)
    {
        LogFilepaths.AddRange(filepaths);
        return this;
    }

    public LogSearcherOptions IncludeLogFilesInDirectory(string directory, string filter = "*", bool recursive = true)
    {
        LogFolders.Add(new LogFolderSource(directory, filter, recursive));
        return this;
    }

    public LogSearcherOptions IncludeLogFilesInDirectories(IEnumerable<string> directories, string filter = "*", bool recursive = true)
    {
        foreach (var directory in directories)
        {
            LogFolders.Add(new LogFolderSource(directory, filter, recursive));
        }
        return this;
    }
}