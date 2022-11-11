using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCPagedJobLogItems
    {
        /// <summary></summary>
        public long TotalCount { get; set; }

        /// <summary></summary>
        public List<HCJobLogItem> Items { get; set; } = new();
    }
}
