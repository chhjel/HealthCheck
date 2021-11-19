using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Viewmodel for <see cref="HCDataRepeaterStreamItemsPagedModel"/>
    /// </summary>
    public class HCDataRepeaterStreamItemsPagedViewModel
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public List<HCDataRepeaterStreamItemViewModel> Items { get; set; }
    }
}
