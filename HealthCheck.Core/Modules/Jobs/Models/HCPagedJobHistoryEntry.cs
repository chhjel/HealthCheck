using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCPagedJobHistoryEntry
    {
        /// <summary></summary>
        public int TotalCount { get; set; }
        /// <summary></summary>
        public List<HCJobHistoryEntry> Items { get; set; }
    }
}
