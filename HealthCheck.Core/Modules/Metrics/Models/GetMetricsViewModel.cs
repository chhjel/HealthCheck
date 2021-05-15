using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Metrics.Models
{
    /// <summary>
    /// Model used when retrieving metrics.
    /// </summary>
    public class GetMetricsViewModel
    {
        /// <summary></summary>
        public Dictionary<string, CompiledMetricsCounterData> GlobalCounters { get; set; } = new();

        /// <summary></summary>
        public Dictionary<string, CompiledMetricsValueData> GlobalValues { get; set; } = new();
    }
}
