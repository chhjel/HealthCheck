using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Caches up to a given number of regex expressions.
    /// </summary>
    public class CachedRegexContainer
    {
        /// <summary>
        /// Defaults to 1000.
        /// </summary>
        public int MaxCount { get; set; } = 1000;

        /// <summary>
        /// Defaults to true.
        /// </summary>
        public bool IgnoreExceptions { get; set; } = true;

        private static readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

        /// <summary>
        /// Get or create a regex with the given pattern.
        /// </summary>
        public Regex GetRegex(string pattern, bool caseSensitive)
        {
            var key = $"{(caseSensitive ? "cs______" : "")}_{pattern}";
            lock (_regexCache)
            {
                if (_regexCache.Count > 1000)
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
}
