using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Models;

/// <summary>
/// Filter options for <see cref="IDataflowStream{TAccessRole}.GetLatestStreamEntriesAsync(DataflowStreamFilter)"/>.
/// </summary>
public class DataflowStreamFilter
{
    /// <summary>
    /// Number of entries to skip.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Number of entries to take.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// If not null, only include from the given date.
    /// </summary>
    public DateTimeOffset? FromDate { get; set; }

    /// <summary>
    /// If not null, only include up until the given date.
    /// </summary>
    public DateTimeOffset? ToDate { get; set; }

    /// <summary>
    /// Filter on property values.
    /// <para>Keys are property names and values are input values.</para>
    /// </summary>
    public Dictionary<string, string> PropertyFilters { get; set; }

    /// <summary>
    /// Get the input filter value for the given property, or the default value if not filter was in place for the property.
    /// </summary>
    public string GetPropertyFilterInput(string propertyName, string defaultValue = null)
    {
        if (PropertyFilters == null || !PropertyFilters.ContainsKey(propertyName))
        {
            return defaultValue;
        }
        else
        {
            return PropertyFilters[propertyName] ?? defaultValue;
        }
    }

    /// <summary>
    /// Filter the given entries on the given property name by any filter input.
    /// </summary>
    public IEnumerable<TEntry> FilterContains<TEntry>(IEnumerable<TEntry> entries, string propertyName,
        Func<TEntry, string> propertyGetter, bool caseSensitive = false)
    {
        var input = GetPropertyFilterInput(propertyName);
        if (string.IsNullOrWhiteSpace(input))
        {
            return entries;
        }
        else if (caseSensitive)
        {
            return entries.Where(x => string.IsNullOrWhiteSpace(input) || propertyGetter(x)?.Contains(input.Trim()) == true);
        }
        else
        {
            return entries.Where(x => string.IsNullOrWhiteSpace(input) || propertyGetter(x)?.ToLower().Contains(input.ToLower().Trim()) == true);
        }
    }

    /// <summary>
    /// Creates a summary of the values to include in audit logging.
    /// </summary>
    public string CreateAuditSummary()
    {
        return SimpleStringifier.Stringify(this);
    }
}
