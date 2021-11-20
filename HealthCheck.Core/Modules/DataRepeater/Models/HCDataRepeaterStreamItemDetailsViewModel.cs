using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Details about an item.
    /// </summary>
    public class HCDataRepeaterStreamItemDetailsViewModel
    {
        /// <summary>
        /// Any relevant description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Any relevant links.
        /// </summary>
        public List<HCDataRepeaterStreamItemHyperLink> Links { get; set; }

        /// <summary>
        /// The item itself.
        /// </summary>
        public HCDataRepeaterStreamItemViewModel Item { get; set; }
    }
}
