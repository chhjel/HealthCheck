using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;

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
    }
}
