﻿using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Metrics.Models
{
    /// <summary>
    /// Metrics data prepared for frontend consumption.
    /// </summary>
    public class CompiledMetricsData
    {
        /// <summary>
        /// Counter values by value type id.
        /// </summary>
        public Dictionary<string, CompiledMetricsCounterData> GlobalCounters { get; set; } = new();

        /// <summary>
        /// Min/max/avg values by value type id.
        /// </summary>
        public Dictionary<string, CompiledMetricsValueData> GlobalValues { get; set; } = new();
    }

}
