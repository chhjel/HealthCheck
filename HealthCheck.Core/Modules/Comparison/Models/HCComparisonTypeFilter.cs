using HealthCheck.Core.Modules.Comparison.Abstractions;

namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Filter passed to <see cref="IHCComparisonTypeHandler.GetFilteredOptionsAsync"/>
    /// </summary>
    public class HCComparisonTypeFilter
    {
        /// <summary>
        /// User input.
        /// </summary>
        public string Input { get; set; }
    }
}
