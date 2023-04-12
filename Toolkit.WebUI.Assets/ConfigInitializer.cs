using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.WebUI.Assets;

internal class ConfigInitializer : ITKExtModuleInitializer
{
    private static bool _initialized = false;
    private static void SetInitialized() => _initialized = true;

    /// <summary>
    /// Invoked from <see cref="TKGlobalConfig"/> constructor.
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
        if (!TKAssetGlobalConfig.EnableInitializer)
        {
            return;
        }

        LoadAssetData();

        TKAssetGlobalConfig.DefaultJavaScriptUrls = new List<string> {
            "[base]/GetAsset?n=toolkit.js&v=[TKVERSION]"
        };
        TKAssetGlobalConfig.DefaultCssUrls = new List<string> {
            "[base]/GetAsset?n=toolkit.css&v=[TKVERSION]",
            "[base]/GetAsset?n=google_fonts.css&v=[TKVERSION]",
            "[base]/GetAsset?n=monaco_editor.main.min.css&v=[TKVERSION]", // not needed anymore? included in main bundle?
            "[base]/GetAsset?n=fontawesome.css&v=[TKVERSION]"
        };
        //TKAssetGlobalConfig.DefaultMetricsSummaryJavascriptUrl = "[base]/GetAsset?n=metrics.js";
        //TKAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl = "[base]/GetAsset?n=releaseNotesSummary.js";
        TKAssetGlobalConfig.DefaultEditorWorkerUrl = "[base]/GetAsset?n=editor.worker.js&v=[TKVERSION]";
        TKAssetGlobalConfig.DefaultJsonWorkerUrl = "[base]/GetAsset?n=json.worker.js&v=[TKVERSION]";
        TKAssetGlobalConfig.DefaultSqlWorkerUrl = "[base]/GetAsset?n=sql.worker.js&v=[TKVERSION]";
    }

    private static readonly Regex _cssUrlRegex = new(@"(?<prefix>url\()(?<url>[^\)]+)(?<suffix>\))");
    private static void LoadAssetData()
    {
        try
        {
            TKAssetGlobalConfig.AssetCache = new();
            TKAssetGlobalConfig.BinaryAssetCache = new();

            addAssetsFromResource("assets.zip");
            // vuetify.min.css
            // google_fonts.css
            // monaco_editor.main.min.css
            // fontawesome.css
            // fa-solid-900.woff2
            // fa-regular-400.woff2
            // materialicons.woff2
            // montserrat.woff2
            addAssetsFromResource("assets_ext.zip");

            static void addAssetsFromResource(string assetName)
            {
                using var stream = GetEmbeddedResourceStream(assetName);
                using var zipArchive = new ZipArchive(stream);

                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.Name.EndsWith(".js") || entry.Name.EndsWith(".css"))
                    {
                        var content = readContents(entry);

                        // Edit css to reference bundled fonts
                        if (entry.Name == "fontawesome.css")
                        {
                            content = content.Replace("../webfonts/", "GetAsset?v=[TKVERSION]&n=");
                        }
                        else if (entry.Name == "google_fonts.css")
                        {
                            foreach (var match in _cssUrlRegex.Matches(content).Cast<Match>())
                            {
                                var prefix = match.Groups["prefix"].Value;
                                var url = match.Groups["url"].Value;
                                var suffix = match.Groups["suffix"].Value;

                                string newUrl = null;
                                if (url.Contains("materialicons")) newUrl = "GetAsset?v=[TKVERSION]&n=materialicons.woff2";
                                else if (url.Contains("montserrat")) newUrl = "GetAsset?v=[TKVERSION]&n=montserrat.woff2";

                                if (newUrl != null)
                                {
                                    var newValue = $"{prefix}{newUrl}{suffix}";
                                    content = content.Replace(match.Value, newValue);
                                }
                            }
                        }

                        TKAssetGlobalConfig.AssetCache[entry.Name] = content;
                    }
                    else if (entry.Name.EndsWith(".woff2"))
                    {
                        var content = readContentBytes(entry);
                        TKAssetGlobalConfig.BinaryAssetCache[entry.Name] = content;
                    }
                }
            }

            static string readContents(ZipArchiveEntry entry)
            {
                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream);
                return reader.ReadToEnd();
            }
            static byte[] readContentBytes(ZipArchiveEntry entry)
            {
                using var entryStream = entry.Open();
                var memoryStream = new MemoryStream();
                entryStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(ConfigInitializer), nameof(LoadAssetData), ex);
        }
    }

    private static Stream GetEmbeddedResourceStream(string filename, string namespacename = "QoDL.Toolkit.WebUI.Assets")
    {
        var assembly = typeof(ConfigInitializer).Assembly;
        var resourceName = namespacename + "." + filename;
        return assembly.GetManifestResourceStream(resourceName);
    }

    private static byte[] GetEmbeddedResourceBytes(string filename, string namespacename = "QoDL.Toolkit.WebUI.Assets")
    {
        using var stream = GetEmbeddedResourceStream(namespacename, filename);
        if (stream == null) return null;
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        return bytes;
    }
}
