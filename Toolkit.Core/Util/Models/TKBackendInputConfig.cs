using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Models;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Util.Models;

/// <summary></summary>
public class TKBackendInputConfig
{
    /// <summary></summary>
    public string Type { get; set; }

    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary></summary>
    public bool Nullable { get; set; }

    /// <summary></summary>)
    public string DefaultValue { get; set; }

    /// <summary></summary>)
    public string NullName { get; set; }

    /// <summary></summary>)
    public string TextPattern { get; set; }

    /// <summary></summary>)
    public string CodeLanguage { get; set; }

    /// <summary></summary>
    public List<string> Flags { get; set; }

    /// <summary></summary>
    public List<TKUIHint> UIHints { get; set; }

    /// <summary></summary>
    public List<string> PossibleValues { get; set; }

    /// <summary></summary>
    [TKRtProperty(ForcedType = "number | null")]
    public int? ParameterIndex { get; set; }

    /// <summary></summary>
    public Dictionary<string, string> ExtraValues { get; set; }

    /// <summary></summary>
    internal PropertyInfo PropertyInfo { get; set; }

    internal static string EnsureJsRegexIsWrappedIfNotEmpty(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        else if (!input.StartsWith("/")) return @$"/{input}/g";
        else return input;
    }
}
