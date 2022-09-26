using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterBatchedStorageItemActions
    {
        /// <summary></summary>
        public List<HCDataRepeaterBatchedStorageItemAction> Adds { get; set; } = new();

        /// <summary></summary>
        public List<HCDataRepeaterBatchedStorageItemAction> Updates { get; set; } = new();

        /// <summary></summary>
        public List<HCDataRepeaterBatchedStorageItemAction> Deletes { get; set; } = new();
    }
}
