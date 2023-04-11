using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Modules.LogViewer.Models;

/// <summary>
/// Parsed query input.
/// </summary>
public class ParsedQuery
{
    /// <summary>
    /// A list of needles that must be found.
    /// </summary>
    public List<string> MustContain { get; set; } = new List<string>();

    /// <summary>
    /// A list of needles that must be found at least one of.
    /// </summary>
    public List<string[]> MustContainOneOf { get; set; } = new List<string[]>();

    /// <summary>
    /// If input mode is regex this will be set to the input pattern.
    /// </summary>
    public string RegexPattern { get; set; }

    /// <summary>
    /// True if regex mode is enabled.
    /// </summary>
    public bool IsRegex => Regex != null;

    /// <summary>
    /// Any error during the parsing of the input.
    /// </summary>
    public string ParseError { get; set; }

    internal Regex Regex { get; set; }

    internal bool AllowItem(string input, bool negate)
    {
        if (!MustContain.Any() && !MustContainOneOf.Any() && !IsRegex)
        {
            return true;
        }

        if (IsRegex)
        {
            var regexResult = Regex.IsMatch(input);
            if (negate)
            {
                regexResult = !regexResult;
            }
            return regexResult;
        }

        if (!MustContain.All(x => input.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
        {
            return negate;
        }
        else if (!MustContainOneOf.All(groups => groups.Any(word => input.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) != -1)))
        {
            return negate;
        }

        return !negate;
    }
}
