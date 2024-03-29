using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Config;

/// <summary>
/// Some global configs for the TK web UI and summary assets.
/// </summary>
public static class TKAssetGlobalConfig
{
    /// <summary>
    /// Default to true. If false the <c>QoDL.Toolkit.WebUI.Assets</c> nuget wont have any effect.
    /// </summary>
    public static bool EnableInitializer { get; set; } = true;

    /// <summary>
    /// Is set from <c>QoDL.Toolkit.WebUI.Assets</c> to load assets from memory instead of CDN.
    /// </summary>
    public static Dictionary<string, string> AssetCache { get; set; }

    /// <summary>
    /// Is set from <c>QoDL.Toolkit.WebUI.Assets</c> to load assets from memory instead of CDN.
    /// </summary>
    public static Dictionary<string, byte[]> BinaryAssetCache { get; set; }

    /// <summary>
    /// Sets global default value for <c>TKPageOptions.JavaScriptUrls</c>.
    /// </summary>
    public static List<string> DefaultJavaScriptUrls { get; set; } = new()
    {
        "https://unpkg.com/christianw-toolkit@2/toolkit.js"
    };

    /// <summary>
    /// Sets global default value for <c>TKPageOptions.CssUrls</c>.
    /// </summary>
    public static List<string> DefaultCssUrls { get; set; } = new()
    {
        "https://unpkg.com/christianw-toolkit@2/toolkit.css",
        "https://fonts.googleapis.com/css?family=Montserrat|Material+Icons",
        "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.19.2/min/vs/editor/editor.main.min.css",
        "https://use.fontawesome.com/releases/v5.15.4/css/all.css"
    };

    /// <summary>
    /// Sets global default value for <c>TKFrontEndOptions.EditorConfig</c><c>.EditorWorkerUrl</c>.
    /// </summary>
    public static string DefaultEditorWorkerUrl { get; set; } = "blob:https://unpkg.com/christianw-toolkit@2/editor.worker.js";

    /// <summary>
    /// Sets global default value for <c>TKFrontEndOptions.EditorConfig</c><c>.JsonWorkerUrl</c>.
    /// </summary>
    public static string DefaultJsonWorkerUrl { get; set; } = "blob:https://unpkg.com/christianw-toolkit@2/json.worker.js";

    /// <summary>
    /// Sets global default value for <c>TKFrontEndOptions.EditorConfig</c><c>.DefaultSqlWorkerUrl</c>.
    /// </summary>
    public static string DefaultSqlWorkerUrl { get; set; } = "blob:https://unpkg.com/christianw-toolkit@2/sql.worker.js";

    /// <summary>
    /// Sets global default value for <c>TKFrontEndOptions.EditorConfig</c><c>.DefaultHtmlWorkerUrl</c>.
    /// </summary>
    public static string DefaultHtmlWorkerUrl { get; set; } = "blob:https://unpkg.com/christianw-toolkit@2/html.worker.js";

    /// <summary></summary>
    public static string DefaultMetricsSummaryJavascriptUrl { get; set; } = "https://unpkg.com/christianw-toolkit@2/metrics.js";

    /// <summary></summary>
    public static string DefaultReleaseNotesSummaryJavascriptUrl { get; set; } = "https://unpkg.com/christianw-toolkit@2/releaseNotesSummary.js";

    internal static string EndpointBase { get; set; }

    /// <summary>
    /// Creates html of the default css urls, or <paramref name="configured"/> if provided.
    /// </summary>
    public static string CreateCssTags(string endpointBase, List<string> configured, IEnumerable<string> additionalUrls = null)
    {
        var urls = configured ?? new List<string>();
        if (urls.Count == 0) urls = DefaultCssUrls ?? new List<string>();
        urls = urls
            .Concat(additionalUrls ?? Enumerable.Empty<string>())
            .Where(x => x != null)
            .Select(x => x.Replace("[base]", endpointBase?.TrimEnd('/') ?? ""))
            .ToList();
        return CreateCssTags(urls);
    }

    /// <summary>
    /// Creates css tags from the given urls.
    /// </summary>
    public static string CreateCssTags(List<string> urls)
        => string.Join("\n", (urls ?? Enumerable.Empty<string>())
            .Where(x => x != null)
            .Select(x =>
            {
                if (x.StartsWith("<")) return x;
                var attributes = x.StartsWith("/") ? string.Empty : "crossorigin=\"anonymous\" ";
                return $"<link rel=\"stylesheet\" href=\"{x}\" {attributes}/>";
            }));

    /// <summary>
    /// Creates html of the default JavaScript urls, or <paramref name="configured"/> if provided.
    /// </summary>
    public static string CreateJavaScriptTags(string endpointBase, List<string> configured, IEnumerable<string> additionalUrls = null)
    {
        var urls = configured ?? new List<string>();
        if (urls.Count == 0) urls = DefaultJavaScriptUrls ?? new List<string>();
        urls = urls
            .Concat(additionalUrls ?? Enumerable.Empty<string>())
            .Where(x => x != null)
            .Select(x => x.Replace("[base]", endpointBase?.TrimEnd('/') ?? ""))
            .ToList();
        return CreateJavaScriptTags(urls);
    }

    /// <summary>
    /// Creates js tags from the given urls.
    /// </summary>
    public static string CreateJavaScriptTags(List<string> urls)
        => string.Join("\n", (urls ?? Enumerable.Empty<string>())
            .Where(x => x != null)
            .Select(x => $"\t<script src=\"{x}\"></script>"));
}
