using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.SiteEvents.Models;

/// <summary>
/// Frontend config for <see cref="TKSiteEventsModule"/>.
/// </summary>
public class TKSiteEventsModuleFrontendOptionsModel
{
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

/// <summary>
/// Config for the different sections.
/// </summary>
public class TKSiteEventsModuleSectionOptions
{
    /// <summary>
    /// Config for the status section.
    /// </summary>
    public TKSiteEventsModuleStatusOptions Status { get; set; } = new();

    /// <summary>
    /// Config for the ongoing events section.
    /// </summary>
    public TKSiteEventsModuleOngoingEventsOptions OngoingEvents { get; set; } = new();

    /// <summary>
    /// Config for the recent events section.
    /// </summary>
    public TKSiteEventsModuleRecentEventsOptions RecentEvents { get; set; } = new();

    /// <summary>
    /// Options for the calendar section.
    /// </summary>
    public TKSiteEventsModuleCalendarOptions Calendar { get; set; } = new();
}

/// <summary>
/// Config for the status section.
/// </summary>
public class TKSiteEventsModuleStatusOptions
{
    /// <summary>
    /// Disable to hide it.
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Config for the ongoing events section.
/// </summary>
public class TKSiteEventsModuleOngoingEventsOptions
{
    /// <summary>
    /// Disable to hide it.
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Number of minutes past the end of a site event it will be displayed below "Current status" on the status page.
    /// <para>Defaults to 30 minutes.</para>
    /// </summary>
    public int BufferMinutes { get; set; } = 30;
}

/// <summary>
/// Config for the recent events section.
/// </summary>
public class TKSiteEventsModuleRecentEventsOptions
{
    /// <summary>
    /// Disable to hide it.
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Max number of latest days to include data from.
    /// <para>Defaults to 3.</para>
    /// </summary>
    public int MaxNumberOfDays { get; set; } = 3;
}

/// <summary>
/// Config for the calendar history section.
/// </summary>
public class TKSiteEventsModuleCalendarOptions
{
    /// <summary>
    /// Disable to hide it.
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Allowed modes to display calendar data in.
    /// <para>Defaults to empty = allow any mode.</para>
    /// </summary>
    public List<TKSiteEventsModuleCalendarMode> AllowedModes { get; set; }

    /// <summary>
    /// Initial mode to show the calendar in.
    /// </summary>
    public TKSiteEventsModuleCalendarMode InitialMode { get; set; } = TKSiteEventsModuleCalendarMode.Month;

    /// <summary>Different ways to display calendar data.</summary>
    public enum TKSiteEventsModuleCalendarMode
    {
        /// <summary></summary>
        Month = 0,
        /// <summary></summary>
        Week,
        /// <summary></summary>
        Day,
        /// <summary></summary>
        List
    }
}
