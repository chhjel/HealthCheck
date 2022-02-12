using HealthCheck.Core.Modules.DataRepeater.Abstractions;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Result from individial items from <see cref="IHCDataRepeaterStreamItemBatchAction.ExecuteBatchActionAsync"/>
    /// </summary>
    public class HCDataRepeaterStreamItemBatchActionResult
    {
        /// <summary>
        /// Result status from attempted item update.
        /// </summary>
        public ItemActionResultStatus Status { get; set; }

        /// <summary>
        /// If true, the job will stop after this item.
        /// </summary>
        public bool StopBatchJob { get; internal set; }

        /// <summary>
        /// Result status from an attempted item update.
        /// </summary>
        public enum ItemActionResultStatus
        {
            /// <summary></summary>
            UpdatedSuccessfully,
            /// <summary></summary>
            NotAttemptedUpdated,
            /// <summary></summary>
            UpdateFailed
        }

        /// <summary>Applies any changes.</summary>
        public static HCDataRepeaterStreamItemBatchActionResult CreateSuccess(bool stopBatchJob = false)
            => new() { Status = ItemActionResultStatus.UpdatedSuccessfully, StopBatchJob = stopBatchJob };

        /// <summary>Skips this item.</summary>
        public static HCDataRepeaterStreamItemBatchActionResult CreateNotAttemptedUpdated(bool stopBatchJob = false)
            => new() { Status = ItemActionResultStatus.NotAttemptedUpdated, StopBatchJob = stopBatchJob };

        /// <summary>Marks the item as failed and applies any changes.</summary>
        public static HCDataRepeaterStreamItemBatchActionResult CreateUpdateFailed(bool stopBatchJob = false)
            => new() { Status = ItemActionResultStatus.UpdateFailed, StopBatchJob = stopBatchJob };
    }
}
