using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// N amount of br-tags.
/// </summary>
public class HtmlPresetSpace : HtmlPresetRaw
{
    /// <summary>
    /// N amount of br-tags.
    /// </summary>
    public HtmlPresetSpace(int count = 1)
        : base(string.Join("", Enumerable.Range(1, count).Select(x => "<br />"))) { }
}