namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Model sent to frontend.
    /// </summary>
    public class TKComparisonDifferDefinition
    {
        /// <summary>
        /// Generated id of the differ.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the differ.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of the differ.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// True if the differ should be enabled by default.
        /// </summary>
        public bool EnabledByDefault { get; set; }
    }
}
