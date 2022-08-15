using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SiteEvents.Models
{
    /// <summary>
    /// Frontend config for <see cref="HCSiteEventsModule"/>.
    /// </summary>
    public class HCSiteEventsModuleFrontendOptionsModel
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
        public HCSiteEventsModuleSectionOptions Sections { get; set; } = new();
    }

    /// <summary>
    /// Config for the different sections.
    /// </summary>
    public class HCSiteEventsModuleSectionOptions
    {
        /// <summary>
        /// Config for the status section.
        /// </summary>
        public HCSiteEventsModuleStatusOptions Status { get; set; } = new();

        /// <summary>
        /// Config for the ongoing events section.
        /// </summary>
        public HCSiteEventsModuleOngoingEventsOptions OngoingEvents { get; set; } = new();

        /// <summary>
        /// Config for the recent events section.
        /// </summary>
        public HCSiteEventsModuleRecentEventsOptions RecentEvents { get; set; } = new();

        /// <summary>
        /// Options for the calendar section.
        /// </summary>
        public HCSiteEventsModuleCalendarOptions Calendar { get; set; } = new();
    }

    /// <summary>
    /// Config for the status section.
    /// </summary>
    public class HCSiteEventsModuleStatusOptions
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
    public class HCSiteEventsModuleOngoingEventsOptions
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
    public class HCSiteEventsModuleRecentEventsOptions
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
    public class HCSiteEventsModuleCalendarOptions
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
        public List<HCSiteEventsModuleCalendarMode> AllowedModes { get; set; }

        /// <summary>
        /// Initial mode to show the calendar in.
        /// </summary>
        public HCSiteEventsModuleCalendarMode InitialMode { get; set; } = HCSiteEventsModuleCalendarMode.Month;

        /// <summary>Different ways to display calendar data.</summary>
        public enum HCSiteEventsModuleCalendarMode
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
}
