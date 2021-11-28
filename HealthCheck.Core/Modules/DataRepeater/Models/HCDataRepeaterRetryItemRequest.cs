using System;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterRetryItemRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary></summary>
        public Guid ItemId { get; set; }

        /// <summary></summary>
        public string SerializedDataOverride { get; set; }
    }
}
