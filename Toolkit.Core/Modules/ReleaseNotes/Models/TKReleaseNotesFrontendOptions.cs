using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Frontend config for <see cref="TKReleaseNotesModule"/>.
    /// </summary>
    public class TKReleaseNotesFrontendOptions
    {
        /// <summary>
        /// Any additional links to display at the top, optionally with access role requirements.
        /// </summary>
        public List<TKReleaseNoteLinkWithAccess> TopLinks { get; set; } = new();
    }
}
