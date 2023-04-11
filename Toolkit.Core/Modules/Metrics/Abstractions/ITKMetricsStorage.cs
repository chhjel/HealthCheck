using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Modules.Metrics.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Metrics.Abstractions
{
    /// <summary>
    /// Stores and retrieves metrics.
    /// </summary>
    public interface ITKMetricsStorage
    {
        /// <summary>
        /// Store data from the given metrics context.
        /// </summary>
        Task StoreMetricDataAsync(TKMetricsContext data);

        /// <summary>
        /// Get metrics data prepared for frontend consumption.
        /// </summary>
        Task<CompiledMetricsData> GetCompiledMetricsDataAsync();
    }
}
