using System.Collections.Generic;

namespace HealthCheck.Core.Config
{
    /// <summary>
    /// Some global configs for the HC web UI and summary assets.
    /// </summary>
    public static class HCAssetGlobalConfig
    {
        /// <summary>
        /// Default to true. If false the <c>HealthCheck.WebUI.Assets</c> nuget wont have any effect.
        /// </summary>
        public static bool EnableInitializer { get; set; } = true;

        /// <summary>
        /// Is set from <c>HealthCheck.WebUI.Assets</c> to load assets from memory instead of CDN.
        /// </summary>
        public static Dictionary<string, string> AssetCache { get; set; }

        /// <summary>
        /// Sets global default value for <c>HCPageOptions.JavaScriptUrls</c>.
        /// </summary>
        public static List<string> DefaultJavaScriptUrls { get; set; } = new()
        {
            "https://unpkg.com/christianh-healthcheck@2/healthcheck.js"
        };

        /// <summary>
        /// Sets global default value for <c>HCPageOptions.CssUrls</c>.
        /// </summary>
        public static List<string> DefaultCssUrls { get; set; } = new()
        {
            "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.19.2/min/vs/editor/editor.main.min.css"
        };

        /// <summary>
        /// Sets global default value for <c>HCFrontEndOptions.EditorConfig</c><c>.EditorWorkerUrl</c>.
        /// </summary>
        public static string DefaultEditorWorkerUrl { get; set; } = "blob:https://unpkg.com/christianh-healthcheck@2/editor.worker.js";

        /// <summary>
        /// Sets global default value for <c>HCFrontEndOptions.EditorConfig</c><c>.JsonWorkerUrl</c>.
        /// </summary>
        public static string DefaultJsonWorkerUrl { get; set; } = "blob:https://unpkg.com/christianh-healthcheck@2/json.worker.js";

        /// <summary></summary>
        public static string DefaultMetricsSummaryJavascriptUrl { get; set; } = "https://unpkg.com/christianh-healthcheck@2/metrics.js";

        /// <summary></summary>
        public static string DefaultReleaseNotesSummaryJavascriptUrl { get; set; } = "https://unpkg.com/christianh-healthcheck@2/releaseNotesSummary.js";
    }
}
