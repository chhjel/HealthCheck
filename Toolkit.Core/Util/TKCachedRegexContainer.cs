using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Caches up to a given number of regex expressions.
/// </summary>
public class TKCachedRegexContainer
{
    /// <summary>
    /// Defaults to 1000.
    /// </summary>
    public int MaxCount { get; set; } = 1000;

    /// <summary>
    /// Defaults to true.
    /// </summary>
    public bool IgnoreExceptions { get; set; } = true;

    private static readonly Dictionary<string, Regex> _regexCache = new();

    /// <summary>
    /// Get or create a regex with the given pattern.
    /// </summary>
    public Regex GetRegex(string pattern, bool caseSensitive)
    {
        var key = $"{(caseSensitive ? "cs______" : "")}_{pattern}";
        lock (_regexCache)
        {
            if (_regexCache.Count > MaxCount)
            {
                _regexCache.Clear();
            }

            if (!_regexCache.ContainsKey(key))
            {
                Regex regex = null;
                try
                {
                    regex = new Regex(pattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
                catch (Exception)
                {
                    if (!IgnoreExceptions)
                    {
                        throw;
                    }
                    return null;
                }

                try
                {
                    _regexCache[key] = regex;
                }
                catch (Exception)
                {
                    if (!IgnoreExceptions)
                    {
                        throw;
                    }
                    return regex;
                }
            }
            return _regexCache[key];
        }
    }
}
