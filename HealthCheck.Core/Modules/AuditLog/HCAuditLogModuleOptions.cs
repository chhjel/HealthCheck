using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Util;
using System;

namespace HealthCheck.Core.Modules.AuditLog
{
    /// <summary>
    /// Options for <see cref="HCAuditLogModule"/>.
    /// </summary>
    public class HCAuditLogModuleOptions
    {
        /// <summary>
        /// Must be set for any site audits to be logged.
        /// </summary>
        public IAuditEventStorage AuditEventService { get; set; }

        /// <summary>
        /// If set to true, client ip and useragent will be included in all stored events and not just in selected ones.
        /// </summary>
        public bool IncludeClientConnectionDetailsInAllEvents { get; set; }

        /// <summary>
        /// Optional logic for stripping any sensitive data.
        /// <para>Various usefull util methods can be found in <see cref="HCSensitiveDataUtils"/>.</para>
        /// </summary>
        public StripSensitiveDataDelegate SensitiveDataStripper { get; set; }

        /// <summary>
        /// Signature for data stripping <see cref="SensitiveDataStripper"/>.
        /// </summary>
        public delegate string StripSensitiveDataDelegate(string input);
    }
}
