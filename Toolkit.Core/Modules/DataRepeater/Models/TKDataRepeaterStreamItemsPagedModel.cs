using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Response from <see cref="ITKDataRepeaterStreamItemStorage.GetItemsPagedAsync"/>
    /// </summary>
    public class TKDataRepeaterStreamItemsPagedModel
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public IEnumerable<ITKDataRepeaterStreamItem> Items { get; set; }
    }
}
