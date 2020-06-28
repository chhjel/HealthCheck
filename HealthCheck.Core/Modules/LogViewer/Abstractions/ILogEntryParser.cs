using HealthCheck.Core.Modules.LogViewer.Models;
using System;

namespace HealthCheck.Core.Modules.LogViewer.Abstractions
{
    internal interface ILogEntryParser
    {
        DateTimeOffset? ParseEntryDate(string line);
        bool IsEntryStart(string line);
        LogEntry ParseDetails(LogEntry entry);
    }
}