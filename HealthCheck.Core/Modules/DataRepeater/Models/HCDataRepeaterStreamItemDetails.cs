using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Any additional details about an item.
    /// </summary>
    public class HCDataRepeaterStreamItemDetails
    {
        /// <summary>
        /// Any relevant description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Any relevant links.
        /// </summary>
        public List<HCDataRepeaterStreamItemHyperLink> Links { get; set; }
    }
}
