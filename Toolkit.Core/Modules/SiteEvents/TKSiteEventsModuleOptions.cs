using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;

namespace QoDL.Toolkit.Core.Modules.SiteEvents;

/// <summary>
/// Options for <see cref="TKSiteEventsModule"/>.
/// </summary>
public class TKSiteEventsModuleOptions
{
    /// <summary>
    /// Handles events for the overview page.
    /// </summary>
    public ISiteEventService SiteEventService { get; set; }

    /// <summary>
    /// Optional custom html to display on the top of the page.
    /// </summary>
    public string CustomHtml { get; set; }

    /// <summary>
    /// Optionally show filter in frontend.
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool ShowFilter { get; set; } = true;

    /// <summary>
    /// Number of seconds to configure the frontend timer with for the optional page auto-refreshes.
    /// <para>Defaults to 30 seconds.</para>
    /// </summary>
    public int FrontendAutoRefreshSecondsInterval { get; set; } = 30;

    /// <summary>
    /// Config for the different sections.
    /// </summary>
    public TKSiteEventsModuleSectionOptions Sections { get; set; } = new();
}
