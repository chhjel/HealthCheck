using QoDL.Toolkit.Core.Modules.LogViewer.Abstractions;
using QoDL.Toolkit.Core.Modules.LogViewer.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Util;

internal class LogEntryParser : ILogEntryParser
{
    private Regex ColumnRegex { get; set; }
    private string ColumnDelimiter { get; set; }

    public LogEntryParser(Regex columnRegex = null, string columnDelimiter = null)
    {
        ColumnRegex = columnRegex;
        ColumnDelimiter = columnDelimiter;
    }

    public bool IsEntryStart(string line)
    {
        return ParseEntryDate(line) != null;
    }

    public LogEntry ParseDetails(LogEntry entry)
    {
        var raw = entry.Raw;

        string[] columnValues = null;
        if (ColumnDelimiter != null)
        {
            columnValues = raw.Split(new[] { ColumnDelimiter }, StringSplitOptions.None);
        }
        else if (ColumnRegex != null)
        {
            var columnMatch = ColumnRegex.Match(raw);
            columnValues = columnMatch.Groups
                .Cast<Group>()
                .Skip(1)
                .Select(x => x.Success ? x.Value : null).ToArray();
        }

        entry.ColumnValues = columnValues;
        return entry;
    }

    public DateTimeOffset? ParseEntryDate(string line)
    {
        if (string.IsNullOrWhiteSpace(line?.Trim()))
        {
            return null;
        }

        DateTimeOffset? date = null;

        var tabbedParts = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (DateTimeOffset.TryParse(tabbedParts[0].Trim(), out DateTimeOffset parsedDate))
        {
            date = parsedDate;
        }

        if (date == null)
        {
            var spacedParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (DateTimeOffset.TryParse(tabbedParts[0].Trim(), out parsedDate))
            {
                date = parsedDate;
            }
            else if (spacedParts.Length >= 2 && DateTimeOffset.TryParse($"{spacedParts[0]} {spacedParts[1]}".Trim(), out parsedDate))
            {
                date = parsedDate;
            }
            else if (spacedParts.Length >= 2 && spacedParts[1].Contains(",")
                && DateTimeOffset.TryParse($"{spacedParts[0]} {spacedParts[1].Substring(0, spacedParts[1].IndexOf(","))}".Trim(), out parsedDate))
            {
                date = parsedDate;
            }
        }

        return date;
    }
}