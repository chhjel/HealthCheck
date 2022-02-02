using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace HealthCheck.WebUI.Assets
{
    internal class ConfigInitializer : IHCExtModuleInitializer
    {
        private static bool _initialized = false;
        private static void SetInitialized() => _initialized = true;

        /// <summary>
        /// Invoked from <see cref="HCGlobalConfig"/> constructor.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            SetInitialized();

            InitConfig();
        }

        private static void InitConfig()
        {
            if (!HCAssetGlobalConfig.EnableInitializer)
            {
                return;
            }

            var assets = LoadAssetData();

            HCAssetGlobalConfig.AssetCache = assets;

            HCAssetGlobalConfig.DefaultJavaScriptUrls = new List<string> {
                "[base]/GetAsset?n=healthcheck.js"
            };
            //HCAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl = "[base]/GetAsset?n=metrics.js";
            //HCAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl = "[base]/GetAsset?n=releaseNotesSummary.js";
            HCAssetGlobalConfig.DefaultEditorWorkerUrl = "[base]/GetAsset?n=editor.worker.js";
            HCAssetGlobalConfig.DefaultJsonWorkerUrl = "[base]/GetAsset?n=json.worker.js";
        }

        private static Dictionary<string, string> LoadAssetData()
        {
            try
            {
                using var stream = GetEmbeddedResourceStream("assets.zip");
                using var zipArchive = new ZipArchive(stream);

                Dictionary<string, string> assets = new();
                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.Name.EndsWith(".js") || entry.Name.EndsWith(".css"))
                    {
                        assets[entry.Name] = readContents(entry);
                    }
                }

                return assets;

                static string readContents(ZipArchiveEntry entry)
                {
                    using var entryStream = entry.Open();
                    using var reader = new StreamReader(entryStream);
                    return reader.ReadToEnd();
                }
            } catch(Exception ex)
            {
                HCGlobalConfig.OnExceptionEvent?.Invoke(typeof(ConfigInitializer), nameof(LoadAssetData), ex);
                return new();
            }
        }

        private static Stream GetEmbeddedResourceStream(string filename, string namespacename = "HealthCheck.WebUI.Assets")
        {
            var assembly = typeof(ConfigInitializer).Assembly;
            var resourceName = namespacename + "." + filename;
            return assembly.GetManifestResourceStream(resourceName);
        }

        private static byte[] GetEmbeddedResourceBytes(string filename, string namespacename = "HealthCheck.WebUI.Assets")
        {
            using var stream = GetEmbeddedResourceStream(namespacename, filename);
            if (stream == null) return null;
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
