using QoDL.Toolkit.Core.Modules.Metrics.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Metrics
{
    /// <summary>
    /// Options for <see cref="TKMetricsModule"/>.
    /// </summary>
    public class TKMetricsModuleOptions
    {
        /// <summary>
        /// Where to get stored metrics to display from.
        /// </summary>
        public ITKMetricsStorage Storage { get; set; }
    }
}
