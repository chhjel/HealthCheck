using QoDL.Toolkit.Core.Modules.LogViewer.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Util
{
    internal static class QueryParser
    {
        private static readonly Regex ExactRegex = new(@"([^\\]|^)""(?<contents>[^""]+?)""([^\\]|$)");
        private static readonly Regex AnyRegex = new(@"\((?<contents>[^\(]+\|[^\)]+)\)");

        public static ParsedQuery ParseQuery(string input, bool isRegex)
        {
            var result = new ParsedQuery();
            if (string.IsNullOrWhiteSpace(input))
            {
                return result;
            }

            if (isRegex)
            {
                try
                {
                    var regexp = new Regex(input, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    result.Regex = regexp;
                    result.RegexPattern = input;
                }
                catch(Exception ex)
                {
                    result.ParseError = ex.Message;
                }
                return result;
            }

            try
            {
                var remainingInput = input;

                var exactMatches = ExactRegex.Matches(remainingInput).Cast<Match>();
                foreach (var match in exactMatches)
                {
                    remainingInput = remainingInput.Replace(match.Value, string.Empty);
                }

                var anyMatches = AnyRegex.Matches(remainingInput).Cast<Match>();
                foreach (var match in anyMatches)
                {
                    remainingInput = remainingInput.Replace(match.Value, string.Empty);
                }

                remainingInput = remainingInput.Replace("\\\"", "\"");

                var all = remainingInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var exact = exactMatches.Select(x => x.Groups["contents"].Value);
                var any = anyMatches.Select(x => x.Groups["contents"].Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

                result.MustContain.AddRange(all);
                result.MustContain.AddRange(exact);
                result.MustContainOneOf.AddRange(any);
            }
            catch(Exception)
            {
                result.MustContain.Add(input);
            }

            return result;
        }
    }
}
