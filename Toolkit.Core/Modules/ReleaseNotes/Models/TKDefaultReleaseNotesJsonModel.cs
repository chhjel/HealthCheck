using QoDL.Toolkit.Core.Modules.ReleaseNotes.Providers;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Default json file parsed by <see cref="TKJsonFileReleaseNotesProvider"/>.
    /// </summary>
    public class TKDefaultReleaseNotesJsonModel
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary></summary>
        public DateTimeOffset? builtAt { get; set; }

        /// <summary></summary>
        public string builtCommitHash { get; set; }

        /// <summary></summary>
        public string version { get; set; }

        /// <summary></summary>
        public List<TKDefaultReleaseNotesChangeJsonModel> changes { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
