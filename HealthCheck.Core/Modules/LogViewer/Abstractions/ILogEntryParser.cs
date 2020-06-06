using HealthCheck.Core.Modules.LogViewer.Models;
using System;

namespace HealthCheck.Core.Modules.LogViewer.Abstractions
{
    internal interface ILogEntryParser
    {
        DateTimeOffset? ParseEntryDate(string raw);
        bool IsEntryStart(string raw);
        LogEntry ParseDetails(LogEntry entry);
    }
}