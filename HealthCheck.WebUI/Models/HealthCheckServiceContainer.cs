﻿using HealthCheck.Core.Abstractions;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Services related to the HealthCheck namespace.
    /// </summary>
    public class HealthCheckServiceContainer
    {
        /// <summary>
        /// Must be set for any site statuses to be stored and returned.
        /// </summary>
        public ISiteEventService SiteEventService { get; set; }

        /// <summary>
        /// Must be set for any site audits to be logged.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }

        /// <summary>
        /// Must be set for the log viewer tab to be displayed.
        /// </summary>
        public ILogSearcherService LogSearcherService { get; set; }
    }
}
