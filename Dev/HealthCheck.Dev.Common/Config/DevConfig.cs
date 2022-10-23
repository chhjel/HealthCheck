using HealthCheck.Core.Config;
using System.Collections.Generic;

namespace HealthCheck.Dev.Common.Config
{
    public static class DevConfig
    {
        public static void ConfigureLocalAssetUrls(string endpointBase = "")
        {
            // Set to true to test using assets from WebUI.Assets zip resources
            var useWebUIAssets = false;

            HCAssetGlobalConfig.EnableInitializer = useWebUIAssets;

            if (!useWebUIAssets)
            {
                HCAssetGlobalConfig.DefaultJavaScriptUrls = new List<string> {
                    "[base]/GetMainScript"
                };
                HCAssetGlobalConfig.DefaultCssUrls.RemoveAt(0);
                HCAssetGlobalConfig.DefaultCssUrls.Insert(0, "[base]/GetMainStyle");
                HCAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl = $"{endpointBase}/GetMetricsScript";
                HCAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl = $"{endpointBase}/GetReleaseNotesScript";
                HCAssetGlobalConfig.DefaultEditorWorkerUrl = "[base]/getscript?name=editor.worker.js";
                HCAssetGlobalConfig.DefaultJsonWorkerUrl = "[base]/getscript?name=json.worker.js";
                HCAssetGlobalConfig.DefaultSqlWorkerUrl = "[base]/getscript?name=sql.worker.js";
            }
        }
    }
}
