﻿using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Contains data about a single change.
    /// </summary>
    public class HCReleaseNoteChangeViewModel
    {
        /// <summary>
        /// Title above of the change.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional description of the change if any.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional icon of the change.
        /// <para>Value should be a constant from <see cref="MaterialIcons"/>.</para>
        /// <para>Defaults to <c>MaterialIcons.AllIcons.Fiber_Manual_Record</c></para>
        /// </summary>
        public string Icon { get; set; } = MaterialIcons.AllIcons.Fiber_Manual_Record;

        /// <summary>
        /// Optionally include time when the change was committed.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Optionally include related commit hash.
        /// </summary>
        public string CommitHash { get; set; }

        /// <summary>
        /// Optionally include author of this change.
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Optional link when the change is clicked.
        /// </summary>
        public string MainLink { get; set; }

        /// <summary>
        /// Optionally any additional links this change should point to.
        /// </summary>
        public IEnumerable<HCReleaseNoteLinkViewModel> Links { get; set; }
    }
}
