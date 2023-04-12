using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Modules.DataRepeater.Utils;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Core.Modules.DataRepeater.Models.TKDataRepeaterStreamItemBatchActionResult;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Services;

/// <summary>
/// Handles repeater streams for reprocessing data.
/// </summary>
public class TKDataRepeaterService : ITKDataRepeaterService
{
    /// <summary>
    /// If disabled the service will ignore any attempts to store new data.
    /// <para>Enabled by default. Null value/exception = false.</para>
    /// </summary>
    public Func<bool> IsEnabled { get; set; } = () => true;

    private readonly IEnumerable<ITKDataRepeaterStream> _streams;

    /// <summary>
    /// Handles repeater streams for reprocessing data.
    /// </summary>
    public TKDataRepeaterService(IEnumerable<ITKDataRepeaterStream> streams)
    {
        _streams = streams;
    }

    /// <inheritdoc />
    public IEnumerable<ITKDataRepeaterStream> GetStreams() => _streams;

    /// <inheritdoc />
    public virtual async Task AddStreamItemAsync<TStream>(ITKDataRepeaterStreamItem item, object hint = null, bool analyze = true, bool handleDuplicates = true)
    {
        if (!IsEnabledInternal()) return;
        if (item == null) return;
        item.Log ??= new();

        var stream = GetStreams()?.FirstOrDefault(x => x.GetType() == typeof(TStream));
        if (stream == null) return;

        // Analyze if enabled
        if (analyze)
        {
            var analyticResult = await stream.AnalyzeItemAsync(item, isManualAnalysis: false);
            if (analyticResult != null)
            {
                if (analyticResult.DontStore)
                {
                    return;
                }
                TKDataRepeaterUtils.ApplyChangesToItem(item, analyticResult);
            }
        }

        var existingItem = handleDuplicates
            ? await stream.Storage.GetItemByItemIdAsync(item.ItemId)
            : null;

        // Nothing to merge, add and return
        if (existingItem == null)
        {
            await stream.Storage.AddItemAsync(item, hint);
            return;
        }

        // Handle merging
        var mergeResult = await stream.HandleAddedDuplicateItemAsync(existingItem, item);

        // Handle old item
        if (mergeResult.OldItemAction == TKDataRepeaterItemMergeConflictResult.OldItemActionType.Delete)
        {
            await stream.Storage.DeleteItemAsync(existingItem.Id);
        }
        else if (mergeResult.OldItemAction == TKDataRepeaterItemMergeConflictResult.OldItemActionType.Update)
        {
            await stream.Storage.UpdateItemAsync(existingItem);
        }

        // Handle new item
        if (mergeResult.NewItemAction == TKDataRepeaterItemMergeConflictResult.NewItemActionType.Insert)
        {
            await stream.Storage.AddItemAsync(item, hint);
        }
        else if (mergeResult.NewItemAction == TKDataRepeaterItemMergeConflictResult.NewItemActionType.Ignore)
        {
            /* Ignored */
        }
    }

    /// <inheritdoc />
    public virtual async Task AddStreamItemsAsync<TStream>(IEnumerable<ITKDataRepeaterStreamItem> items, object hint = null, bool analyze = true, bool handleDuplicates = true)
    {
        if (!IsEnabledInternal()) return;
        if (items?.Any() != true) return;

        var stream = GetStreams()?.FirstOrDefault(x => x.GetType() == typeof(TStream));
        if (stream == null) return;

        var existingItems = (await stream.Storage.GetAllItemsAsync())
            .ToDictionaryIgnoreDuplicates(x => x.ItemId ?? string.Empty, x => x);

        var itemsActions = new TKDataRepeaterBatchedStorageItemActions();
        foreach (var item in items)
        {
            item.Log ??= new();

            // Analyze if enabled
            if (analyze)
            {
                var analyticResult = await stream.AnalyzeItemAsync(item, isManualAnalysis: false);
                if (analyticResult != null)
                {
                    if (analyticResult.DontStore)
                    {
                        return;
                    }
                    TKDataRepeaterUtils.ApplyChangesToItem(item, analyticResult);
                }
            }

            var existingItem = (handleDuplicates && existingItems.TryGetValue(item.ItemId, out var exItem)) ? exItem : null;

            // Nothing to merge, add and return
            if (existingItem == null)
            {
                itemsActions.Adds.Add(new TKDataRepeaterBatchedStorageItemAction(item, hint));
                continue;
            }

            // Handle merging
            var mergeResult = await stream.HandleAddedDuplicateItemAsync(existingItem, item);

            // Handle old item
            if (mergeResult.OldItemAction == TKDataRepeaterItemMergeConflictResult.OldItemActionType.Delete)
            {
                itemsActions.Deletes.Add(new TKDataRepeaterBatchedStorageItemAction(existingItem, null));
            }
            else if (mergeResult.OldItemAction == TKDataRepeaterItemMergeConflictResult.OldItemActionType.Update)
            {
                itemsActions.Updates.Add(new TKDataRepeaterBatchedStorageItemAction(existingItem, null));
            }

            // Handle new item
            if (mergeResult.NewItemAction == TKDataRepeaterItemMergeConflictResult.NewItemActionType.Insert)
            {
                itemsActions.Adds.Add(new TKDataRepeaterBatchedStorageItemAction(item, hint));
            }
            else if (mergeResult.NewItemAction == TKDataRepeaterItemMergeConflictResult.NewItemActionType.Ignore)
            {
                /* Ignored */
            }
        }

        await stream.Storage.PerformBatchUpdateAsync(itemsActions);
    }

    /// <inheritdoc />
    public virtual async Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(string streamId, ITKDataRepeaterStreamItem item)
    {
        var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
        if (stream == null)
        {
            return new TKDataRepeaterItemAnalysisResult { Message = "Stream not found." };
        }

        item.Log ??= new();

        // Handle any exception
        TKDataRepeaterItemAnalysisResult result = null;
        try
        {
            result = await stream.AnalyzeItemAsync(item, isManualAnalysis: true);
            if (result == null)
            {
                return new TKDataRepeaterItemAnalysisResult { Message = "Analysis returned null." };
            }
        }
        catch (Exception ex)
        {
            item.LastRetryWasSuccessful = false;
            item.AddLogMessage($"Analysis was attempted. Failed with exception: {ex.Message}");
            await stream.Storage.UpdateItemAsync(item);
            return new TKDataRepeaterItemAnalysisResult
            {
                Message = TKExceptionUtils.GetFullExceptionDetails(ex)
            };
        }

        // Update log
        var statusMessage = result.Message;
        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            statusMessage = "Executed successfully.";
        }
        item.AddLogMessage($"Analysis was attempted. Result: {statusMessage}");

        // Apply AllowRetry and tag changes
        TKDataRepeaterUtils.ApplyChangesToItem(item, result);

        // Save changes
        await stream.Storage.UpdateItemAsync(item);

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<TKDataRepeaterRetryResult> RetryItemAsync(string streamId, ITKDataRepeaterStreamItem item)
    {
        var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
        if (stream == null)
        {
            return TKDataRepeaterRetryResult.CreateError("Stream not found.");
        }

        item.LastRetriedAt = DateTimeOffset.Now;
        item.Log ??= new();

        // Handle any exception
        TKDataRepeaterRetryResult result = null;
        try
        {
            result = await stream.RetryItemAsync(item);
        }
        catch(Exception ex)
        {
            item.LastRetryWasSuccessful = false;
            item.AddLogMessage($"Retry was attempted. Failed with exception: {ex.Message}");
            await stream.Storage.UpdateItemAsync(item);
            return TKDataRepeaterRetryResult.CreateError(ex);
        }

        // Update last success and message
        item.LastRetryWasSuccessful = result.Success;
        var statusMessage = result.Message;
        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            statusMessage = result.Success ? "Retry was successful" : "Retry failed.";
        }
        item.AddLogMessage($"Retry was attempted. Result: {statusMessage}");

        // Apply AllowRetry and tag changes
        TKDataRepeaterUtils.ApplyChangesToItem(item, result);

        if (result.Delete)
        {
            await stream.Storage.DeleteItemAsync(item.Id);
        }
        else
        {
            // Save changes
            await stream.Storage.UpdateItemAsync(item);
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<TKDataRepeaterStreamItemActionResult> PerformItemAction(string streamId, string actionId,
        ITKDataRepeaterStreamItem item, Dictionary<string, string> parameters)
    {
        var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
        if (stream == null)
        {
            return TKDataRepeaterStreamItemActionResult.CreateError($"Stream '{streamId}' not found.");
        }

        ITKDataRepeaterStreamItemAction action = stream.Actions?.FirstOrDefault(x => x?.GetType()?.FullName == actionId);
        if (action == null)
        {
            return TKDataRepeaterStreamItemActionResult.CreateError($"Action '{actionId}' not found.");
        }

        var allowedResult = await action.ActionIsAllowedForAsync(item);
        if (allowedResult?.Allowed != true)
        {
            return TKDataRepeaterStreamItemActionResult.CreateError(string.IsNullOrWhiteSpace(allowedResult.Reason) 
                ? $"Item '{item.ItemId}' does not allow for the action to be executed."
                : allowedResult.Reason);
        }

        object parametersObject = action.ParametersType == null ? null : TKValueConversionUtils.ConvertInputModel(action.ParametersType, parameters);

        item.LastActionAt = DateTimeOffset.Now;
        item.Log ??= new();

        // Handle any exception
        TKDataRepeaterStreamItemActionResult result = null;
        try
        {
            result = await action.ExecuteActionAsync(stream, item, parametersObject);
        }
        catch (Exception ex)
        {
            item.AddLogMessage($"Action '{action.DisplayName}' was executed. Failed with exception: {ex.Message}");
            await stream.Storage.UpdateItemAsync(item);
            return TKDataRepeaterStreamItemActionResult.CreateError(ex);
        }

        // Log message
        var statusMessage = result.Message;
        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            statusMessage = result.Success ? "Action was successful" : "Action failed.";
        }
        item.AddLogMessage($"Action '{action.DisplayName}' was executed. Result: {statusMessage}");

        // Apply AllowRetry and tag changes
        TKDataRepeaterUtils.ApplyChangesToItem(item, result);

        if (result.Delete)
        {
            await stream.Storage.DeleteItemAsync(item.Id);
        }
        else
        {
            // Save changes
            await stream.Storage.UpdateItemAsync(item);
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<TKDataRepeaterStreamBatchActionResult> PerformItemBatchAction(string streamId, string actionId, Dictionary<string, string> parameters)
    {
        var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
        if (stream == null)
        {
            return TKDataRepeaterStreamBatchActionResult.CreateError($"Stream '{streamId}' not found.");
        }

        var action = stream.BatchActions?.FirstOrDefault(x => x?.GetType()?.FullName == actionId);
        if (action == null)
        {
            return TKDataRepeaterStreamBatchActionResult.CreateError($"Action '{actionId}' not found.");
        }

        object parametersObject = action.ParametersType == null ? null : TKValueConversionUtils.ConvertInputModel(action.ParametersType, parameters);

        var batchResult = new TKDataRepeaterStreamBatchActionResult();
        var items = (await stream.Storage.GetAllItemsAsync()).ToArray();
        foreach (var item in items)
        {
            item.LastActionAt = DateTimeOffset.Now;
            item.Log ??= new();

            // Handle any exception
            try
            {
                var itemResult = await action.ExecuteBatchActionAsync(item, parametersObject, batchResult);
                if (itemResult == null || itemResult.Status == ItemActionResultStatus.NotAttemptedUpdated)
                {
                    batchResult.NotAttemptedUpdatedCount++;
                    continue;
                }

                // Log message
                item.AddLogMessage($"Batch action '{action.DisplayName}' was executed. Result: {itemResult.Status.ToString().SpacifySentence()}");
                await stream.Storage.UpdateItemAsync(item);

                if (itemResult.Status == ItemActionResultStatus.UpdatedSuccessfully)
                {
                    batchResult.UpdatedSuccessfullyCount++;
                }
                else if (itemResult.Status == ItemActionResultStatus.UpdateFailed)
                {
                    batchResult.UpdateFailedCount++;
                }

                if (itemResult.StopBatchJob)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                batchResult.UpdateFailedCount++;
                item.AddLogMessage($"Batch action '{action.DisplayName}' was executed. Failed on this item with exception: {ex.Message}");
                await stream.Storage.UpdateItemAsync(item);
            }
        }

        return batchResult;
    }

    internal bool IsEnabledInternal()
    {
        try
        {
            if (IsEnabled?.Invoke() != true)
            {
                return false;
            }
        }
        catch (Exception) { return false; }

        return true;
    }
}
