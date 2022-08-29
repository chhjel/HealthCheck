using HealthCheck.Core.Modules.Comparison.Abstractions;

namespace HealthCheck.Core.Modules.Comparison
{
    /// <summary>
    /// Options for <see cref="HCComparisonModule"/>.
    /// </summary>
    public class HCComparisonModuleOptions
    {
        /// <summary>
        /// Service that handles the comparing.
        /// </summary>
        public IHCComparisonService Service { get; set; }
    }
}
