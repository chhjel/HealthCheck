using HealthCheck.Core.Modules.SiteEvents.Abstractions;

namespace HealthCheck.Core.Modules.SiteEvents
{
    /// <summary>
    /// Options for <see cref="HCSiteEventsModule"/>.
    /// </summary>
    public class HCSiteEventsModuleOptions
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
        /// Number of minutes past the end of a site event it will be displayed below "Current status" on the status page.
        /// <para>Defaults to 30 minutes.</para>
        /// </summary>
        public int CurrentEventBufferMinutes { get; set; } = 30;

        /// <summary>
        /// Number of seconds to configure the frontend timer with for the optional page auto-refreshes.
        /// <para>Defaults to 30 seconds.</para>
        /// </summary>
        public int FrontendAutoRefreshSecondsInterval { get; set; } = 30;
    }
}
