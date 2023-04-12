using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary>
/// Result from individial items from <see cref="ITKDataRepeaterStreamItemBatchAction.ExecuteBatchActionAsync"/>
/// </summary>
public class TKDataRepeaterStreamItemBatchActionResult
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
    public static TKDataRepeaterStreamItemBatchActionResult CreateSuccess(bool stopBatchJob = false)
        => new() { Status = ItemActionResultStatus.UpdatedSuccessfully, StopBatchJob = stopBatchJob };

    /// <summary>Skips this item.</summary>
    public static TKDataRepeaterStreamItemBatchActionResult CreateNotAttemptedUpdated(bool stopBatchJob = false)
        => new() { Status = ItemActionResultStatus.NotAttemptedUpdated, StopBatchJob = stopBatchJob };

    /// <summary>Marks the item as failed and applies any changes.</summary>
    public static TKDataRepeaterStreamItemBatchActionResult CreateUpdateFailed(bool stopBatchJob = false)
        => new() { Status = ItemActionResultStatus.UpdateFailed, StopBatchJob = stopBatchJob };
}
