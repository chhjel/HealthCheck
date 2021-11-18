using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Filter model sent to <see cref="IHCDataRepeaterStream.GetItemsPagedAsync"/>
    /// </summary>
    public class HCGetDataRepeaterStreamItemsFilteredRequest
    {
        /// <summary>
        /// Item id to search for.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Page index to start at.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size to return.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tags to include, or empty to include all.
        /// </summary>
        public List<string> Tags { get; set; }
    }
}
