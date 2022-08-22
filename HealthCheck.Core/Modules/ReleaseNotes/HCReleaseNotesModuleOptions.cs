using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;
using HealthCheck.Core.Modules.ReleaseNotes.Models;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes
{
    /// <summary>
    /// Options for <see cref="HCReleaseNotesModule"/>.
    /// </summary>
    public class HCReleaseNotesModuleOptions
    {
        /// <summary>
        /// Provides data to display in the release notes module.
        /// </summary>
        public IHCReleaseNotesProvider ReleaseNotesProvider { get; set; }

        /// <summary>
        /// Any additional links to display at the top, optionally with access role requirements.
        /// </summary>
        public List<HCReleaseNoteLinkWithAccess> TopLinks { get; set; } = new();
    }
}
