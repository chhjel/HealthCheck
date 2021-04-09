﻿using System;

namespace HealthCheck.Core.Modules.Metrics.Models
{
    /// <summary>
    /// Metrics data prepared for frontend consumption.
    /// </summary>
    public class CompiledMetricsValueData
    {
        /// <summary>
        /// Id of this value type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Average value.
        /// </summary>
        public long Average { get; set; }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public long Min { get; set; }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public long Max { get; set; }

        /// <summary>
        /// Number of values values are calculated from.
        /// </summary>
        public long ValueCount { get; set; }

        /// <summary>
        /// When the latest change was reported.
        /// </summary>
        public DateTimeOffset LastChanged { get; set; }

        /// <summary></summary>
        public override string ToString() => $"{Id}: {Average}";
    }

}
