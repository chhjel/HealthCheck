using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCJobsStartJobRequestModel
    {
        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public string JobId { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Parameters { get; set; }
    }
}
