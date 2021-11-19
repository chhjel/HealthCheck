using System;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCGetDataRepeaterItemDetailsRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary></summary>
        public Guid ItemGuid { get; set; }

        /// <summary></summary>
        public string ItemId { get; set; }
    }
}
