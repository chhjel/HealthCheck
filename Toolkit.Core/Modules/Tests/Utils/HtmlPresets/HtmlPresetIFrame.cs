namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// An Iframe with the given url and height.
/// </summary>
public class HtmlPresetIFrame : HtmlPresetRaw
{
    /// <summary>
    /// An Iframe with the given url and height.
    /// </summary>
    public HtmlPresetIFrame(string url, string height = "500px")
        : base($"<iframe src=\"{url}\" width=\"100%\" height=\"{height}\"></iframe>") { }
}