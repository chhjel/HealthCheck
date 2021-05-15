using HealthCheck.Core.Modules.Metrics.Abstractions;

namespace HealthCheck.Core.Modules.Metrics
{
    /// <summary>
    /// Options for <see cref="HCMetricsModule"/>.
    /// </summary>
    public class HCMetricsModuleOptions
    {
        /// <summary>
        /// Where to get stored metrics to display from.
        /// </summary>
        public IHCMetricsStorage Storage { get; set; }
    }
}
