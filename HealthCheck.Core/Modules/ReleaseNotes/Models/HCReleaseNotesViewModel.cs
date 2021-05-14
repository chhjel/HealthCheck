using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Contains data to display in the release notes module.
    /// </summary>
    public class HCReleaseNotesViewModel
    {
        /// <summary>
        /// Title above of the notes.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description above the notes.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If this property is set, only this message will be shown and any release notes will be hidden.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Optionally include when the release was built.
        /// </summary>
        public DateTimeOffset? BuiltAt { get; set; }

        /// <summary>
        /// Optionally include when the release was deployed.
        /// </summary>
        public DateTimeOffset? DeployedAt { get; set; }

        /// <summary>
        /// Optionally include the version number/name.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Optionally include commit hash the release was built from.
        /// </summary>
        public string BuiltCommitHash { get; set; }

        /// <summary>
        /// A list of all the changes in the release.
        /// </summary>
        public IEnumerable<HCReleaseNoteChangeViewModel> Changes { get; set; }
    }
}
