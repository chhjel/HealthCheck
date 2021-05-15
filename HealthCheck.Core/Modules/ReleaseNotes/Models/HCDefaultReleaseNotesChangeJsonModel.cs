using HealthCheck.Core.Modules.ReleaseNotes.Providers;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Default json file parsed by <see cref="HCJsonFileReleaseNotesProvider"/>.
    /// </summary>
    public class HCDefaultReleaseNotesChangeJsonModel
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary></summary>
        public DateTimeOffset? timestamp { get; set; }

        /// <summary></summary>
        public string hash { get; set; }

        /// <summary></summary>
        public string authorName { get; set; }

        /// <summary></summary>
        public string authorMail { get; set; }

        /// <summary>Commit message</summary>
        public string message { get; set; }

        /// <summary>Commit message body</summary>
        public string body { get; set; }

        /// <summary>Commit message without any issue ids or pullrequest numbers.</summary>
        public string cleanMessage { get; set; }

        /// <summary>First issue id detected in commit message if any</summary>
        public string issueId { get; set; }

        /// <summary>All issue ids detected in commit message</summary>
        public List<string> issueIds { get; set; }

        /// <summary>Detected pull-request number if any</summary>
        public string pullRequestNumber { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
