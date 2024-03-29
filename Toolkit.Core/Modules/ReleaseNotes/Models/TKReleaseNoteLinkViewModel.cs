using QoDL.Toolkit.Core.Util;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Models;

/// <summary>
/// Model for a single link.
/// </summary>
public class TKReleaseNoteLinkViewModel
{
    /// <summary>
    /// Title of the link.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Where the link points to.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Optional icon of the link.
    /// <para>Value should be a constant from <see cref="TKMaterialIcons"/>.</para>
    /// <para>Defaults to no icon.</para>
    /// </summary>
    public string Icon { get; set; }
}
