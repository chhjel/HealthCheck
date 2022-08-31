using System.Collections.Generic;

namespace HealthCheck.Core.Modules.GoTo.Models
{
    /// <summary>
    /// Something that was resolved.
    /// </summary>
    public class HCGoToResolvedData
    {
        /// <summary>
        /// Name to display.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description to display.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional url of image to display.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Optional location where the result is from, e.g. a title, code or id.
        /// </summary>
        public string ResolvedFrom { get; set; }

        /// <summary>
        /// Relevant urls.
        /// </summary>
        public List<HCGoToResolvedUrl> Urls { get; set; } = new();
    }
}
