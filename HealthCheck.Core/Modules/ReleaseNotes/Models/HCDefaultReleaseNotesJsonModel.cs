using HealthCheck.Core.Modules.ReleaseNotes.Providers;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Default json file parsed by <see cref="HCJsonFileReleaseNotesProvider"/>.
    /// </summary>
    public class HCDefaultReleaseNotesJsonModel
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary></summary>
        public DateTimeOffset? builtAt { get; set; }

        /// <summary></summary>
        public string builtCommitHash { get; set; }

        /// <summary></summary>
        public string version { get; set; }

        /// <summary></summary>
        public List<HCDefaultReleaseNotesChangeJsonModel> changes { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
