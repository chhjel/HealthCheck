namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// A preset to use with <see cref="HtmlPresetBuilder"/>.
/// </summary>
public interface IHtmlPreset
{
    /// <summary>
    /// Create html from the data in this object.
    /// </summary>
    string ToHtml();
}