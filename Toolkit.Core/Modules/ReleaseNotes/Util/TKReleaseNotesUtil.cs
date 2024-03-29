using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions;
using QoDL.Toolkit.Core.Util;
using System.Linq;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes.Util;

/// <summary>
/// Utility related to the TK release notes module.
/// </summary>
public static class TKReleaseNotesUtil
{
    /// <summary>
    /// Url to the javascript included in <see cref="CreateReleaseNotesSummaryHtml"/>.
    /// <para>Defaults to matching version bundle from unpkg.com CDN.</para>
    /// </summary>
    public static string SummaryHtmlJavascriptUrl { get; set; }

    /// <summary>
    /// Create a summary of the current release notes as html, or null if no notes was found.
    /// </summary>
    public static string CreateReleaseNotesSummaryHtml(bool includeDevDetails = false)
    {
        var provider = TKIoCUtils.GetInstance<ITKReleaseNotesProvider>();
        if (provider == null) { return "<!-- Release notes provider not found. -->"; }

        var data = TKAsyncUtils.RunSync(() => provider.GetViewModelAsync());
        if (data == null) { return "<!-- No release notes data found. -->"; }

        var model = includeDevDetails ? data?.WithDevDetails : data?.WithoutDevDetails;
        if (model?.Changes?.Any() != true) { return "<!-- No release notes data found. -->"; }

        var json = TKGlobalConfig.Serializer?.Serialize(model, pretty: false);
        if (json == null) { return "<!-- Serializer not set. -->"; }

        var jsUrl = SummaryHtmlJavascriptUrl ?? TKAssetGlobalConfig.DefaultReleaseNotesSummaryJavascriptUrl;
        return $@"
                <div id=""rn_00bfcf84-3633-411e-acd2-b9398d252da7"" data-ctx-data=""{HttpUtility.HtmlAttributeEncode(json)}""></div>
                <script src=""{jsUrl}""></script>
";
    }
}
