using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterPerformItemActionRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Type of the action.
        /// </summary>
        public string ActionId { get; set; }

        /// <summary></summary>
        public Guid ItemId { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Parameters { get; set; }
    }
}
