using System;

namespace QoDL.Toolkit.Core.Modules.SiteEvents.Models
{
    /// <summary>
    /// Request model sent to <see cref="TKSiteEventsModule.GetSiteEvents"/>
    /// </summary>
    public class GetSiteEventsRequestModel
    {
        /// <summary>
        /// From datetime.
        /// </summary>
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// To datetime.
        /// </summary>
        public DateTimeOffset? To { get; set; }
    }
}
