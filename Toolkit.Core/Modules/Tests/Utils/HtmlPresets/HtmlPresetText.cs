using System;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// A p-tag with the given text.
/// </summary>
public class HtmlPresetText : IHtmlPreset
{
    /// <summary>
    /// Text in the paragraph.
    /// </summary>
    protected string Text { get; set; }

    /// <summary>
    /// If true newlines will be converted to br-tags.
    /// </summary>
    protected bool ConvertNewlinesToBr { get; set; }

    /// <summary>
    /// A p-tag with the given text.
    /// </summary>
    public HtmlPresetText(string text, bool convertNewlinesToBr = true)
    {
        Text = text;
        ConvertNewlinesToBr = convertNewlinesToBr;
    }

    /// <summary>
    /// Create html from the data in this object.
    /// </summary>
    public string ToHtml()
    {
        if (String.IsNullOrWhiteSpace(Text))
        {
            return String.Empty;
        }

        var text = HttpUtility.HtmlEncode(Text);
        if (ConvertNewlinesToBr)
        {
            text = text.Replace("\n", "<br />");
        }

        return $"<p>{text}</p>";
    }
}
