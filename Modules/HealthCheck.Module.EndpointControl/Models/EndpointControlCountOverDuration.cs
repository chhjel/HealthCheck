using System;
using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Represents a count over duration.
    /// </summary>
    public class EndpointControlCountOverDuration
    {
        /// <summary></summary>
        public long Count { get; set; }

        /// <summary></summary>
        public TimeSpan Duration { get; set; }
    }
}
