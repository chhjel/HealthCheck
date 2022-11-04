using System;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobHistoryEntry
    {
        /// <summary></summary>
        public Guid Id { get; set; }

        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public string JobId { get; set; }

        /// <summary></summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary></summary>
        public Guid? DetailId { get; set; }

        /// <summary></summary>
        public string Summary { get; set; }
    }
}
