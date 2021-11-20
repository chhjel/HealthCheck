using System;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Simple log entry.
    /// </summary>
    public class HCDataRepeaterSimpleLogEntry
    {
        /// <summary>
        /// Time of insertion.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Some details.
        /// </summary>
        public string Message { get; set; }
    }
}
