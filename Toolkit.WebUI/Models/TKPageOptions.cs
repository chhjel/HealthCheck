using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.WebUI.Exceptions;
using System.Collections.Generic;

namespace QoDL.Toolkit.WebUI.Models;

/// <summary>
/// Various options for the page content.
/// </summary>
public class TKPageOptions
{
    /// <summary>
    /// Url to the javascripts for the UI.
    /// <para>Defaults to matching versions bundle from unpkg.com CDN.</para>
    /// <para>If changed, also update <see cref="TKFrontEndOptions.EditorConfig"/> urls.</para>
    /// <para>[base] can be used as a replacement for <c>EndpointBase</c></para>
    /// </summary>
    public List<string> JavaScriptUrls { get; set; } = new List<string>();

    /// <summary>
    /// Url to the assets for the UI.
    /// <para>Defaults to CDN url.</para>
    /// </summary>
    public List<string> CssUrls { get; set; } = new List<string>();

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
    /// Include meta name="robots" content="noindex"
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool IncludeNoIndex { get; set; } = true;

    /// <summary>
    /// Validates values and throws <see cref="ConfigValidationException"/> if things are missing.
    /// </summary>
    public void Validate()
    {
        if ((JavaScriptUrls == null || JavaScriptUrls.Count == 0)
            && (TKAssetGlobalConfig.DefaultJavaScriptUrls == null || TKAssetGlobalConfig.DefaultJavaScriptUrls.Count == 0))
            throw new ConfigValidationException($"Both {nameof(JavaScriptUrls)} and TKAssetGlobalConfig.DefaultJavaScriptUrls is empty.");
    }
}
