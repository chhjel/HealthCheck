using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Models;

/// <summary>
/// A custom filter input entry.
/// </summary>
public class EventSinkNotificationConfigFilter
{
    /// <summary>
    /// Property name to match against, or null if matching against stringified payload value.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Filter input for contains/match/regex matching.
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// Type of matching logic.
    /// </summary>
    public FilterMatchType MatchType { get; set; } = FilterMatchType.Contains;

    /// <summary>
    /// Enable to make matches case sensitive. False by default.
    /// </summary>
    public bool CaseSensitive { get; set; }

    private static readonly TKCachedRegexContainer _regexCache = new();

    /// <summary>
    /// Returns true if defined filters match the input.
    /// </summary>
    public bool IsAllowed(bool payloadIsComplex, string stringifiedPayload, Dictionary<string, string> payloadProperties)
    {
        if (payloadIsComplex && !string.IsNullOrWhiteSpace(PropertyName))
        {
            var key = payloadProperties.FirstOrDefault(x => x.Key?.ToLower().Trim() == PropertyName?.Trim()?.ToLower()).Key;
            if (key == null)
            {
                Console.WriteLine($"A1");
                return false;
            }

            var propValue = payloadProperties[key];
            return ValueMatchesFilter(propValue);
        }
        else
        {
            return ValueMatchesFilter(stringifiedPayload);
        }
    }

    private bool ValueMatchesFilter(string value)
    {
        var filter = Filter;
        value ??= "";
        filter ??= "";

        if (!CaseSensitive)
        {
            value = value?.ToLower();
            filter = filter?.ToLower();
        }

        if (MatchType == FilterMatchType.Contains)
        {
            return value.Contains(filter);
        }
        else if (MatchType == FilterMatchType.Matches)
        {
            return value.Trim() == filter.Trim();
        }
        else if (MatchType == FilterMatchType.StartsWith)
        {
            return value.Trim().StartsWith(filter.Trim());
        }
        else if (MatchType == FilterMatchType.EndsWith)
        {
            return value.Trim().EndsWith(filter.Trim());
        }
        else if (MatchType == FilterMatchType.RegEx)
        {
            var regex = _regexCache.GetRegex(Filter, CaseSensitive);
            if (regex == null)
            {
                return false;
            }
            return regex.IsMatch(value);
        }
        return true;
    }

    /// <summary>
    /// Type of matching logic.
    /// </summary>
    public enum FilterMatchType
    {
        /// <summary>
        /// Checks if value contains filter.
        /// </summary>
        Contains = 0,

        /// <summary>
        /// Matches 1-to-1.
        /// </summary>
        Matches,

        /// <summary>
        /// Checks if value matches filter as regex.
        /// </summary>
        RegEx,

        /// <summary>
        /// Value starts with.
        /// </summary>
        StartsWith,

        /// <summary>
        /// Value ends with.
        /// </summary>
        EndsWith
    }
}
