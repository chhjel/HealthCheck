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
    }
}
