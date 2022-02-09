using HealthCheck.Core.Util;

namespace HealthCheck.Core.Modules.ReleaseNotes.Models
{
    /// <summary>
    /// Model for a single link.
    /// </summary>
    public class HCReleaseNoteLinkViewModel
    {
        /// <summary>
        /// Title of the link.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Where the link points to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Optional icon of the link.
        /// <para>Value should be a constant from <see cref="HCMaterialIcons"/>.</para>
        /// <para>Defaults to no icon.</para>
        /// </summary>
        public string Icon { get; set; }
    }
}
