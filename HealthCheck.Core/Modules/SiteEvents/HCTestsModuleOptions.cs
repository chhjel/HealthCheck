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
        /// Number of minutes past the end of a site event it will be displayed below "Current status" on the status page.
        /// <para>Defaults to 30 minutes.</para>
        /// </summary>
        public int CurrentEventBufferMinutes { get; set; } = 30;
    }
}
