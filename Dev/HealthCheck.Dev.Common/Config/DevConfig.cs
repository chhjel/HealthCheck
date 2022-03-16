using HealthCheck.Core.Config;
using System.Collections.Generic;

namespace HealthCheck.Dev.Common.Config
{
    public static class DevConfig
    {
        public static void ConfigureLocalAssetUrls(string endpointBase = "")
        {
            HCAssetGlobalConfig.EnableInitializer = false;

            HCAssetGlobalConfig.DefaultJavaScriptUrls = new List<string> {
                "[base]/GetMainScript"
            };
            HCAssetGlobalConfig.DefaultCssUrls.RemoveAt(0);
            HCAssetGlobalConfig.DefaultCssUrls.Insert(0, "[base]/GetMainStyle");
            HCAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl = $"{endpointBase}/GetMetricsScript";
            HCAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl = $"{endpointBase}/GetReleaseNotesScript";
            HCAssetGlobalConfig.DefaultEditorWorkerUrl = "[base]/getscript?name=editor.worker.js";
            HCAssetGlobalConfig.DefaultJsonWorkerUrl = "[base]/getscript?name=json.worker.js";
        }
    }
}
