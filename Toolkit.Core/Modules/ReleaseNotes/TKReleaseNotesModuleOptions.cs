using QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes;

/// <summary>
/// Options for <see cref="TKReleaseNotesModule"/>.
/// </summary>
public class TKReleaseNotesModuleOptions
{
    /// <summary>
    /// Provides data to display in the release notes module.
    /// </summary>
    public ITKReleaseNotesProvider ReleaseNotesProvider { get; set; }

    /// <summary>
    /// Any additional links to display at the top, optionally with access role requirements.
    /// </summary>
    public List<TKReleaseNoteLinkWithAccess> TopLinks { get; set; } = new();
}
