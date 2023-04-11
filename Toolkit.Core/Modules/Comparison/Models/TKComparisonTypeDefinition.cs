namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary></summary>
    public class TKComparisonTypeDefinition
    {
        /// <summary>
        /// Generated id of the type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of the type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional description shown in dialog where instances are selected.
        /// </summary>
        public string FindInstanceDescription { get; set; }

        /// <summary>
        /// Optional placeholder in filter input in dialog where instances are selected.
        /// </summary>
        public string FindInstanceSearchPlaceholder { get; set; }
    }
}
