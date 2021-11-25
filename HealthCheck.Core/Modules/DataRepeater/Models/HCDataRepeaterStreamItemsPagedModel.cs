using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Response from <see cref="IHCDataRepeaterStreamItemStorage.GetItemsPagedAsync"/>
    /// </summary>
    public class HCDataRepeaterStreamItemsPagedModel
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public IEnumerable<IHCDataRepeaterStreamItem> Items { get; set; }
    }
}
