using System;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobLogItem
    {
        /// <summary></summary>
        public HCJobHistoryStatus? Status { get; set; }

        /// <summary></summary>
        public string Summary { get; set; }

        /// <summary></summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}
