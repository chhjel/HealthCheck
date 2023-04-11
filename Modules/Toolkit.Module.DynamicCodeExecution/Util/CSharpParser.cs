using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Util;

internal class CSharpParser
{
    private const string REGEX_TYPES = @"(?<![ ]=>[ ])[\t\n\r \(;]([a-zA-Z][a-zA-Z0-9]*<?)(?![ ]*=>[ ]*)";
    private const string REGEX_TYPES_CAST = @"\<([a-zA-Z][a-zA-Z0-9]*<?)\>";
    private const string REGEX_PROPERTIES = @"\.([a-zA-Z][a-zA-Z0-9]*<?)[ ]*(?![a-zA-Z0-9\(<])";
    private const string REGEX_METHODS = @"\.([a-zA-Z][a-zA-Z0-9]*<?)[ ]*[\(<]";

    private readonly string[] ReservedKeywords = new[] {
        "abstract", "add", "as", "ascending", "async", "await", "base", "bool", "break", "by", "byte", "case", "catch", "char", "checked", "class",
        "const", "continue", "decimal", "default", "delegate", "descending", "do", "double", "dynamic", "else", "enum", "equals", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "from", "get", "global", "goto", "group", "if", "implicit", "in", "int", "interface",
        "internal", "into", "is", "join", "let", "lock", "long", "namespace", "new", "null", "object", "on", "operator", "orderby", "out", "override",
        "params", "partial", "private", "protected", "public", "readonly", "ref", "remove", "return", "sbyte", "sealed", "select", "set", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "value", "var", "virtual", "void", "volatile", "where", "while", "yield"
    };
    private readonly string[] IgnoredTypes = new[] { "DCEUtils" };
    private readonly string[] IgnoredMethods = new[] { "Dump", "Diff", "SaveDumps" };

    public CodeParseResult Parse(string code)
    {
        return new CodeParseResult()
        {
            Methods = GetMethods(code),
            Properties = GetProperties(code),
            Types = GetTypes(code)
        };
    }

    public class CodeParseResult
    {
        public List<string> Methods { get; set; }
        public List<string> Properties { get; set; }
        public List<string> Types { get; set; }
    }

    private List<string> GetMethods(string code)
    {
        return GetGroupMatches(REGEX_METHODS, code)
            .Where(x => !ReservedKeywords.Contains(x) && !IgnoredMethods.Contains(x))
            .ToList();
    }

    private List<string> GetProperties(string code)
    {
        return GetGroupMatches(REGEX_PROPERTIES, code)
            .Where(x => !ReservedKeywords.Contains(x))
            .ToList();
    }

    private List<string> GetTypes(string code)
    {
        var disallowedPrefixes = new[] { "namespace", "region", "using", "class", "var" };
        var regularTypes = GetGroupMatchesExt(REGEX_TYPES, code)
            .Where(x => !x.IsIgnoredLine && !disallowedPrefixes.Contains(x.PrefixedWord) && !ReservedKeywords.Contains(x.Value))
            .Where(x => x.Value.Length > 1)
            .Select(x => x.Value);
        var castTypes = GetGroupMatches(REGEX_TYPES_CAST, code).Where(x => !ReservedKeywords.Contains(x));

        var list = new List<string>();
        list.AddRange(regularTypes);
        list.AddRange(castTypes);
        list.RemoveAll(x => IgnoredTypes.Contains(x));

        list = list.Select(x =>
        {
            if (x.Contains("<"))
            {
                x += ">";
            }
            return x;
        }).ToList();

        return list;
    }

    private List<string> GetGroupMatches(string pattern, string text, int groupNumber = 2)
    {
        var regex = new Regex(pattern);
        return regex.Matches(text)
            .Cast<Match>()
            .Where(x => x.Groups.Count >= groupNumber)
            .Select(x => x.Groups[groupNumber - 1].Value)
            .ToList();
    }

    private List<GroupMatchExt> GetGroupMatchesExt(string pattern, string text, int groupNumber = 2)
    {
        var regex = new Regex(pattern);
        var groups = regex.Matches(text)
            .Cast<Match>()
            .Where(x => x.Groups.Count >= groupNumber)
            .Select(x => x.Groups[groupNumber - 1]);

        return groups.Select(x => new GroupMatchExt()
        {
            Value = x.Value,
            IsIgnoredLine = IsIgnoredLine(x, text),
            PrefixedWord = GetWordBefore(x, text)
        }).ToList();
    }

    private class GroupMatchExt
    {
        public string Value { get; set; }
        public string PrefixedWord { get; set; }
        public bool IsIgnoredLine { get; set; }
    }

    private string GetWordBefore(Group group, string text)
    {
        text = text.Substring(0, group.Index);
        var pattern = @"(\w+)[ ]+$";
        var regex = new Regex(pattern, RegexOptions.None);
        var prefix = regex.Match(text).Groups[0].Value.Trim();
        return (prefix.Length == 0) ? null : prefix;
    }

    private bool IsIgnoredLine(Group group, string text)
    {
        text = text.Substring(0, group.Index);
        var lines = text.Replace("\r", "").Split('\n');
        var lastLine = lines.Last().Trim();
        return lastLine.StartsWith("#") || lastLine.StartsWith("//");
    }
}
