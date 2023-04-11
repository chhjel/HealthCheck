using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class TKPagedJobHistoryEntryViewModel
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public List<TKJobHistoryEntryViewModel> Items { get; set; } = new();
    }
}
