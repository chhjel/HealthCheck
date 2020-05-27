using HealthCheck.Core.Modules.LogViewer.Abstractions;
using HealthCheck.Core.Modules.LogViewer.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HealthCheck.Core.Modules.LogViewer.Util
{
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

        public DateTime? ParseEntryDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw?.Trim()))
            {
                return null;
            }

            DateTime? date = null;

            var tabbedParts = raw.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (DateTime.TryParse(tabbedParts[0].Trim(), out DateTime parsedDate))
            {
                date = parsedDate;
            }

            if (date == null)
            {
                var spacedParts = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (DateTime.TryParse(tabbedParts[0].Trim(), out parsedDate))
                {
                    date = parsedDate;
                }
                else if (spacedParts.Length >= 2 && DateTime.TryParse($"{spacedParts[0]} {spacedParts[1]}".Trim(), out parsedDate))
                {
                    date = parsedDate;
                }
                else if (spacedParts.Length >= 2 && spacedParts[1].Contains(",")
                    && DateTime.TryParse($"{spacedParts[0]} {spacedParts[1].Substring(0, spacedParts[1].IndexOf(","))}".Trim(), out parsedDate))
                {
                    date = parsedDate;
                }
            }

            return date;
        }
    }
}