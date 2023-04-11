using QoDL.Toolkit.Core.Config;
using System.Collections.Generic;

namespace QoDL.Toolkit.Dev.Common.Config
{
    public static class DevConfig
    {
        public static void ConfigureLocalAssetUrls(string endpointBase = "")
        {
            // Set to true to test using assets from WebUI.Assets zip resources
            var useWebUIAssets = false;

            TKAssetGlobalConfig.EnableInitializer = useWebUIAssets;

            if (!useWebUIAssets)
            {
                TKAssetGlobalConfig.DefaultJavaScriptUrls = new List<string> {
                    "[base]/GetMainScript"
                };
                TKAssetGlobalConfig.DefaultCssUrls.RemoveAt(0);
                TKAssetGlobalConfig.DefaultCssUrls.Insert(0, "[base]/GetMainStyle");
                TKAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl = $"{endpointBase}/GetMetricsScript";
                TKAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl = $"{endpointBase}/GetReleaseNotesScript";
                TKAssetGlobalConfig.DefaultEditorWorkerUrl = "[base]/getscript?name=editor.worker.js";
                TKAssetGlobalConfig.DefaultJsonWorkerUrl = "[base]/getscript?name=json.worker.js";
                TKAssetGlobalConfig.DefaultSqlWorkerUrl = "[base]/getscript?name=sql.worker.js";
            }
        }
    }
}
