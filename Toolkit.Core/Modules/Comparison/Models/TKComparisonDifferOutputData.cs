using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Output from <see cref="ITKComparisonDiffer.CompareInstancesAsync"/>
    /// </summary>
    public class TKComparisonDifferOutputData
    {
        /// <summary>
        /// Title of this data.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Type of the output.
        /// </summary>
        public TKComparisonDiffOutputType DataType { get; set; }

        /// <summary>
        /// Serialized data to display.
        /// </summary>
        public string Data { get; set; }
    }
}
