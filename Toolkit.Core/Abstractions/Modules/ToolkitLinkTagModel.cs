using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Util;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Abstractions.Modules;

/// <summary>
/// Represents a simple link-tag.
/// </summary>
public class ToolkitLinkTagModel
{
    /// <summary>
    /// src-attribute value.
    /// </summary>
    public string Rel { get; set; }

    /// <summary>
    /// type-attribute value.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// href-attribute value.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// Create a link tag for css with the given href.
    /// </summary>
    public static ToolkitLinkTagModel CreateStylesheet(string url)
        => new() { Rel = "stylesheet", Type = "text/css", Href = url };

    /// <summary>
    /// Returns the built link element string.
    /// </summary>
    public override string ToString()
    {
        var attributes = new Dictionary<string, string>()
            {
                { "rel", Rel },
                { "type", Type },
                { "href", Href }
            };

        var attributesString = TKHtmlBuilder.BuildAttributes(attributes)
            .EnsureEndsWithIfNotNullOrEmpty(" ");
        return $"<link {attributesString}/>";
    }
}
