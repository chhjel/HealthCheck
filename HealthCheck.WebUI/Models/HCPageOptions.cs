using HealthCheck.WebUI.Exceptions;
using System.Collections.Generic;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Various options for the page content.
    /// </summary>
    public class HCPageOptions
    {
        /// <summary>
        /// Url to the javascripts for the UI.
        /// <para>Defaults to matching versions of logic + vendor bundles from unpkg.com CDN.</para>
        /// <para>If changed, also update <see cref="HCFrontEndOptions.EditorConfig"/> urls.</para>
        /// </summary>
        public List<string> JavaScriptUrls { get; set; } = new List<string>()
        {
            "https://unpkg.com/christianh-healthcheck@2/healthcheck.js",
            "https://unpkg.com/christianh-healthcheck@2/healthcheck.vendor.js",
        };

        /// <summary>
        /// Url to the assets for the UI.
        /// <para>Defaults to matching versions of logic + vendor bundles from unpkg.com CDN.</para>
        /// </summary>
        public List<string> AssetUrls { get; set; } = new List<string>()
        {
            "https://unpkg.com/christianh-healthcheck@2/codicon.ttf"
        };

        /// <summary>
        /// Title of the page.
        /// </summary>
        public string PageTitle { get; set; } = "Health Check";

        /// <summary>
        /// Any custom head section html.
        /// </summary>
        public string CustomHeadHtml { get; set; }

        /// <summary>
        /// Any custom body section html.
        /// </summary>
        public string CustomBodyHtml { get; set; }

        /// <summary>
        /// Include default fonts and css from CDN.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool IncludeDefaultAssetLinks { get; set; } = true;

        /// <summary>
        /// Include meta name="robots" content="noindex"
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool IncludeNoIndex { get; set; } = true;

        /// <summary>
        /// Validates values and throws <see cref="ConfigValidationException"/> if things are missing.
        /// </summary>
        public void Validate()
        {
            if (JavaScriptUrls == null || JavaScriptUrls.Count == 0)
                throw new ConfigValidationException($"{nameof(JavaScriptUrls)} is empty.");
        }
    }
}
