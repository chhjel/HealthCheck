using System;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// An progress-tag with the given value.
/// </summary>
public class HtmlPresetProgressbar : IHtmlPreset
{
    /// <summary>
    /// Max value of the progress bar.
    /// </summary>
    protected string Max { get; set; }

    /// <summary>
    /// Current value of the progress bar.
    /// </summary>
    protected string Value { get; set; }

    /// <summary>
    /// An progress-tag with the given max/value.
    /// </summary>
    public HtmlPresetProgressbar(string max, string value)
    {
        Max = max;
        Value = value;
    }

    /// <summary>
    /// An progress-tag with the given percentage value.
    /// </summary>
    public HtmlPresetProgressbar(float percentage)
    {
        Max = "100";
        Value = ((int)(percentage * 100f)).ToString();
    }

    /// <summary>
    /// Create html from the data in this object.
    /// </summary>
    public string ToHtml()
    {
        if (String.IsNullOrWhiteSpace(Max) || String.IsNullOrWhiteSpace(Value))
        {
            return String.Empty;
        }

        return $"<progress max=\"{Max}\" value=\"{Value}\"></progress><br />";
    }
}