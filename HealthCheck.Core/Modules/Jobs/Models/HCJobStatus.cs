using System;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobStatus
    {
        /// <summary></summary>
        public string JobId { get; set; }

        /// <summary></summary>
        public string Status { get; set; }

        /// <summary></summary>
        public bool IsRunning { get; set; }

        /// <summary></summary>
        public bool IsEnabled { get; set; }

        /// <summary></summary>
        public DateTimeOffset? NextExecutionScheduledAt { get; set; }

        /// <summary></summary>
        public DateTimeOffset? StartedAt { get; set; }

        /// <summary></summary>
        public DateTimeOffset? EndedAt { get; set; }

        /// <summary></summary>
        public bool? LastRunWasSuccessful { get; set; }
    }
}
