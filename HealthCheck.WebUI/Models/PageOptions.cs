using HealthCheck.WebUI.Exceptions;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Various options for the page content.
    /// </summary>
    public class PageOptions
    {
        /// <summary>
        /// Url to the javascript for the UI.
        /// </summary>
        public string JavaScriptUrl { get; set; }

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
            if (string.IsNullOrWhiteSpace(JavaScriptUrl))
                throw new ConfigValidationException($"{nameof(JavaScriptUrl)} is not set.");
        }
    }
}
