namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Models;

/// <summary>
/// Model for a link to be displayed conditionally.
/// </summary>
public class TKReleaseNoteLinkWithAccess
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
    /// Optional access role required. Set to your desired access roles enum.
    /// </summary>
    public object AccessRequired { get; set; }

    /// <summary>
    /// Model for a link to be displayed conditionally.
    /// </summary>
    public TKReleaseNoteLinkWithAccess(string title, string url, object accessRequired = null)
    {
        Title = title;
        Url = url;
        AccessRequired = accessRequired;
    }
}
