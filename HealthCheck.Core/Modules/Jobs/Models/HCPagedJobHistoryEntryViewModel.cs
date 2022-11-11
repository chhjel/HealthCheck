using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCPagedJobHistoryEntryViewModel
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public List<HCJobHistoryEntryViewModel> Items { get; set; } = new();
    }
}
