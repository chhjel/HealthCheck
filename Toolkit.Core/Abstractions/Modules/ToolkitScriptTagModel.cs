using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Util;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Abstractions.Modules;

/// <summary>
/// Represents a simple script-tag.
/// </summary>
public class ToolkitScriptTagModel
{
    /// <summary>
    /// src-attribute value.
    /// </summary>
    public string Src { get; set; }

    /// <summary>
    /// Inline script content.
    /// </summary>
    public string InlineScript { get; set; }

    /// <summary>
    /// Create a script tag with the given src value.
    /// </summary>
    public static ToolkitScriptTagModel CreateSrc(string url)
        => new() { Src = url };

    /// <summary>
    /// Create a script tag with the given inline script.
    /// </summary>
    public static ToolkitScriptTagModel CreateInline(string script)
        => new() { InlineScript = script };

    /// <summary>
    /// Returns the built script element string.
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(InlineScript))
        {
            return $"<script>{InlineScript}</script>";
        }

        var attributes = new Dictionary<string, string>()
            {
                { "src", Src }
            };

        var attributesString = HtmlBuilder.BuildAttributes(attributes)
            .EnsureEndsWithIfNotNullOrEmpty(" ");
        return $"<script {attributesString}/>";
    }
}
