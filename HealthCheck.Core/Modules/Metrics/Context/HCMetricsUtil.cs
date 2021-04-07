using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.Metrics.Models;
using System;

namespace HealthCheck.Core.Modules.Metrics.Context
{
    /// <summary>
    /// Provides static access to tracking metrics.
    /// </summary>
    public static class HCMetricsUtil
    {
        /// <summary>
        /// Gets the current context.
        /// </summary>
        public static Func<HCMetricsContext> CurrentContextFactory { get; set; }

        /// <summary>
        /// Return true to track any custom metrics for the given request.
        /// <para>Defaults to not tracking anything.</para>
        /// </summary>
        public static Func<HCRequestContext, bool> AllowTrackRequestMetrics { get; set; }

        /// <summary></summary>
        public delegate void OnMetricTracked(HCMetricsContext context);

        /// <summary>
        /// Will be invoked when new metrics are ready.
        /// </summary>
        public static event OnMetricTracked OnRequestMetricsReadyEvent;

        internal static void NotifyNewTrackedMetrics(HCMetricsContext context)
        {
            OnRequestMetricsReadyEvent?.Invoke(context);
            // todo: try ioc service & invoke if any
        }
    }
}
