using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Viewmodel for <see cref="TKDataRepeaterStreamItemsPagedModel"/>
    /// </summary>
    public class TKDataRepeaterStreamItemsPagedViewModel
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public List<TKDataRepeaterStreamItemViewModel> Items { get; set; }
    }
}
