using System;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobHistoryDetailEntry
    {
        /// <summary></summary>
        public Guid Id { get; set; }

        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public string Data { get; set; }
    }
}
