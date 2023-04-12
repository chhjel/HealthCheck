using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.Metrics.Abstractions;
using QoDL.Toolkit.Core.Util;
using System;
using System.Threading.Tasks;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.Metrics.Context;

/// <summary>
/// Provides static access to tracking metrics.
/// </summary>
public static class TKMetricsUtil
{
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public static TKMetricsContext Current => CurrentContextFactory?.Invoke();

    /// <summary>
    /// Factory to get or create the current context.
    /// </summary>
    public static Func<TKMetricsContext> CurrentContextFactory { get; set; }

    /// <summary>
    /// Return true to track any custom metrics for the given request.
    /// <para>Defaults to not tracking anything.</para>
    /// </summary>
    public static Func<TKRequestContext, bool> AllowTrackRequestMetrics { get; set; }

    /// <summary>
    /// Url to the javascript included in <see cref="CreateContextSummaryHtml"/>.
    /// <para>Defaults to matching version bundle from unpkg.com CDN.</para>
    /// </summary>
    public static string SummaryHtmlJavascriptUrl { get; set; }

    /// <summary>
    /// Delegate used by <see cref="OnRequestMetricsReadyEvent"/>.
    /// </summary>
    public delegate void OnMetricTracked(TKMetricsContext context);

    /// <summary>
    /// Will be invoked when new metrics are ready.
    /// </summary>
    public static event OnMetricTracked OnRequestMetricsReadyEvent;

    /// <summary>
    /// Gets the current context as json, or null if no context was found.
    /// </summary>
    public static string GetContextAsJson(bool emptyIfNoData = false)
    {
        var context = Current;
        if (context == null || (emptyIfNoData && !context.ContainsData))
        {
            return null;
        }

        context.EndAllTimingsInternal();
        return TKGlobalConfig.Serializer?.Serialize(context, pretty: false);
    }

    /// <summary>
    /// Create a summary of the current context as html, or null if no context was found.
    /// </summary>
    public static string CreateContextSummaryHtml()
    {
        var json = GetContextAsJson(emptyIfNoData: true);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var jsUrl = SummaryHtmlJavascriptUrl ?? TKAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl;
        return $@"
                <div id=""ctx_02aecea7_e695_4749_bb2a_35e060975968"" data-ctx-data=""{HttpUtility.HtmlAttributeEncode(json)}""></div>
                <script src=""{jsUrl}""></script>
";
    }

    internal static void NotifyNewTrackedMetrics(TKMetricsContext context)
    {
        OnRequestMetricsReadyEvent?.Invoke(context);

        try
        {
            var service = TKIoCUtils.GetInstance<ITKMetricsStorage>();
            if (service != null)
            {
                Task.Run(async () => await service.StoreMetricDataAsync(context));
            }
        }
        catch (Exception)
        {
            // Ignored
        }
    }
}
