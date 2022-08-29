using HealthCheck.Core.Modules.Comparison.Abstractions;

namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Model sent to frontend from <see cref="IHCComparisonTypeHandler.GetFilteredOptionsAsync"/>
    /// </summary>
    public class HCComparisonInstanceSelection
    {
        /// <summary>
        /// Any id that can be resolved by your own logic in <see cref="IHCComparisonTypeHandler.GetInstanceWithIdAsync"/>.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of this instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of this instance.
        /// </summary>
        public string Description { get; set; }
    }
}
