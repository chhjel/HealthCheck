using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using System;
using System.Threading.Tasks;
using System.Web;

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
        public static HCMetricsContext Current => CurrentContextFactory?.Invoke();

        /// <summary>
        /// Factory to get or create the current context.
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

        /// <summary>
        /// Gets the current context as json, or null if no context was found.
        /// </summary>
        public static string GetContextAsJson()
        {
            var context = Current;
            if (context == null)
            {
                return null;
            }

            return TestRunnerService.Serializer.Serialize(context, pretty: false);
        }

        /// <summary>
        /// Create a summary of the current context as html, or null if no context was found.
        /// </summary>
        public static string CreateContextSummaryHtml()
        {
            var json = GetContextAsJson();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var jsUrl = "https://www.google.com?q=todo";
            return $@"
                <div id=""ctx_02aecea7_e695_4749_bb2a_35e060975968"" data-ctx-data=""{HttpUtility.HtmlAttributeEncode(json)}""></div>
<script src=""{jsUrl}""></script>
";
        }

        internal static void NotifyNewTrackedMetrics(HCMetricsContext context)
        {
            OnRequestMetricsReadyEvent?.Invoke(context);

            var service = IoCUtils.GetInstance<IHCMetricsService>();
            if (service != null)
            {
                Task.Run(async () =>  await service.StoreMetricDataAsync(context));
            }
        }
    }
}
