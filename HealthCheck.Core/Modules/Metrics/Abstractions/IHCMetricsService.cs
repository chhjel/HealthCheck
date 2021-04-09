using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Metrics.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Metrics.Abstractions
{
    /// <summary>
    /// Stores and retrieves metrics.
    /// </summary>
    public interface IHCMetricsService
    {
        /// <summary>
        /// Store data from the given metrics context.
        /// </summary>
        Task StoreMetricDataAsync(HCMetricsContext data);

        /// <summary>
        /// Get metrics data prepared for frontend consumption.
        /// </summary>
        Task<CompiledMetricsData> GetCompiledMetricsDataAsync();
    }
}
