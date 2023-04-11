using QoDL.Toolkit.Core.Util;

namespace QoDL.Toolkit.Module.EndpointControl.Models
{
    /// <summary>
    /// A filter for a property.
    /// </summary>
    public class EndpointControlPropertyFilter
    {
        /// <summary>
        /// Disable to not filter anything.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Filter value.
        /// </summary>
        public string Filter { get; set; } = "";

        /// <summary>
        /// Filter mode.
        /// </summary>
        public EndpointControlFilterMode FilterMode { get; set; } = EndpointControlFilterMode.Matches;

        /// <summary>
        /// Invert filter output.
        /// </summary>
        public bool Inverted { get; set; }

        /// <summary>
        /// Does case matter?
        /// </summary>
        public bool CaseSensitive { get; set; }

        private static readonly TKCachedRegexContainer _regexCache = new();

        /// <summary>
        /// Returns true if the condition is true for the given value.
        /// </summary>
        public bool Matches(string value)
        {
            if (!Enabled || (Filter ?? "").Length == 0)
            {
                return true;
            }

            return !Inverted ? CheckCondition(value) : !CheckCondition(value);
        }

        private bool CheckCondition(string value)
        {
            var filter = Filter;
            value ??= "";
            filter ??= "";

            if (!CaseSensitive)
            {
                value = value?.ToLower();
                filter = filter?.ToLower();
            }

            if (FilterMode == EndpointControlFilterMode.Contains)
            {
                return value.Contains(filter);
            }
            else if (FilterMode == EndpointControlFilterMode.Matches)
            {
                return value.Trim() == filter.Trim();
            }
            else if (FilterMode == EndpointControlFilterMode.StartsWith)
            {
                return value.StartsWith(filter);
            }
            else if (FilterMode == EndpointControlFilterMode.EndsWith)
            {
                return value.EndsWith(filter);
            }
            else if (FilterMode == EndpointControlFilterMode.RegEx)
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
    }
}
