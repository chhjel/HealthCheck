using HealthCheck.Core.Modules.Comparison.Abstractions;

namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Output from <see cref="IHCComparisonDiffer.CompareInstancesAsync"/>
    /// </summary>
    public class HCComparisonDifferOutputData
    {
        /// <summary>
        /// Title of this data.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Type of the output.
        /// </summary>
        public HCComparisonDiffOutputType DataType { get; set; }

        /// <summary>
        /// Serialized data to display.
        /// </summary>
        public string Data { get; set; }
    }
}
