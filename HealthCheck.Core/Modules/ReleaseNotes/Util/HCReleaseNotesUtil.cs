using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using System.Web;

namespace HealthCheck.Core.Modules.ReleaseNotes.Util
{
    /// <summary>
    /// Utility related to the HC release notes module.
    /// </summary>
    public static class HCReleaseNotesUtil
    {
        /// <summary>
        /// Url to the javascript included in <see cref="CreateReleaseNotesSummaryHtml"/>.
        /// <para>Defaults to matching version bundle from unpkg.com CDN.</para>
        /// </summary>
        public static string SummaryHtmlJavascriptUrl { get; set; } = "https://unpkg.com/christianh-healthcheck@2/release-notes-summary.js";

        /// <summary>
        /// Create a summary of the current release notes as html, or null if no notes was found.
        /// </summary>
        public static string CreateReleaseNotesSummaryHtml(bool includeDevDetails = false)
        {
            var provider = IoCUtils.GetInstance<IHCReleaseNotesProvider>();
            if (provider == null) { return "<!-- Release notes provider not found. -->"; }

            var data = AsyncUtils.RunSync(() => provider.GetViewModelAsync());
            if (data == null) { return "<!-- No release notes data found. -->"; }

            var model = includeDevDetails ? data?.WithDevDetails : data?.WithoutDevDetails;
            if (model == null) { return "<!-- No release notes data found. -->"; }

            var json = HCGlobalConfig.Serializer?.Serialize(model, pretty: false);
            if (json == null) { return "<!-- Serializer not set. -->"; }

            return $@"
                <div id=""rn_00bfcf84-3633-411e-acd2-b9398d252da7"" data-ctx-data=""{HttpUtility.HtmlAttributeEncode(json)}""></div>
                <script src=""{SummaryHtmlJavascriptUrl}""></script>
";
        }
    }
}
