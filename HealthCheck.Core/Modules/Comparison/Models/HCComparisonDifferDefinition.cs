namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Model sent to frontend.
    /// </summary>
    public class HCComparisonDifferDefinition
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
    }
}
