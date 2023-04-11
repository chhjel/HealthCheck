using QoDL.Toolkit.Core.Modules.LogViewer.Models;
using System;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Abstractions
{
    internal interface ILogEntryParser
    {
        DateTimeOffset? ParseEntryDate(string line);
        bool IsEntryStart(string line);
        LogEntry ParseDetails(LogEntry entry);
    }
}