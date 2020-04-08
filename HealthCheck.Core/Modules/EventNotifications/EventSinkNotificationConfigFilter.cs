using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HealthCheck.Core.Modules.EventNotifications
{
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

        private static readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

        private bool ValueMatchesFilter(string value)
        {
            var filter = Filter;
            value = value ?? "";
            filter = filter ?? "";

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
            else if (MatchType == FilterMatchType.RegEx)
            {
                var regex = GetRegexFor(Filter, CaseSensitive);
                if (regex == null)
                {
                    return false;
                }
                return regex.IsMatch(value);
            }
            return true;
        }

        private Regex GetRegexFor(string pattern, bool caseSensitive)
        {
            var key = $"{(caseSensitive ? "cs______" : "")}_{pattern}";
            if (!_regexCache.ContainsKey(key))
            {
                Regex regex = null;
                try {
                    regex = new Regex(pattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                } catch (Exception) {
                    return null;
                }

                try {
                    _regexCache[key] = regex;
                } catch (Exception) {
                    return regex;
                }
            }
            return _regexCache[key];
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
            RegEx
        }
    }
}
