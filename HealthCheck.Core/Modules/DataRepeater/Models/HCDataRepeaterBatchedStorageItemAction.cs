using HealthCheck.Core.Modules.DataRepeater.Abstractions;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterBatchedStorageItemAction
    {
        /// <summary></summary>
        public IHCDataRepeaterStreamItem Item { get; set; }

        /// <summary></summary>
        public object Hint { get; set; }

        /// <summary></summary>
        public HCDataRepeaterBatchedStorageItemAction(IHCDataRepeaterStreamItem item, object hint)
        {
            Item = item;
            Hint = hint;
        }
    }
}
