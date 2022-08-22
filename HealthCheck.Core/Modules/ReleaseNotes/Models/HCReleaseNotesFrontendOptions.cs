using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Frontend config for <see cref="HCReleaseNotesModule"/>.
    /// </summary>
    public class HCReleaseNotesFrontendOptions
    {
        /// <summary>
        /// Any additional links to display at the top, optionally with access role requirements.
        /// </summary>
        public List<HCReleaseNoteLinkWithAccess> TopLinks { get; set; } = new();
    }
}
