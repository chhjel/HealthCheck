using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Extensions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Modules.DataRepeater.Utils;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Services
{
    /// <summary>
    /// Handles repeater streams for reprocessing data.
    /// </summary>
    public class HCDataRepeaterService : IHCDataRepeaterService
    {
        /// <summary>
        /// Max number of log entries to store per item.
        /// <para>Defaults to 20.</para>
        /// </summary>
        private int MaxItemLogEntries { get; set; } = 20;

        private readonly IEnumerable<IHCDataRepeaterStream> _streams;

        /// <summary>
        /// Handles repeater streams for reprocessing data.
        /// </summary>
        public HCDataRepeaterService(IEnumerable<IHCDataRepeaterStream> streams)
        {
            _streams = streams;
        }

        /// <inheritdoc />
        public IEnumerable<IHCDataRepeaterStream> GetStreams() => _streams;

        /// <inheritdoc />
        public virtual async Task AddStreamItemAsync<TStream>(IHCDataRepeaterStreamItem item, object hint = null, bool analyze = true, bool handleDuplicates = true)
        {
            if (item == null) return;
            item.Log ??= new();

            var stream = GetStreams()?.FirstOrDefault(x => x.GetType() == typeof(TStream));
            if (stream == null) return;

            // Analyze if enabled
            if (analyze)
            {
                var analyticResult = await stream.AnalyzeItemAsync(item);
                if (analyticResult != null)
                {
                    if (analyticResult.DontStore)
                    {
                        return;
                    }
                    HCDataRepeaterUtils.ApplyChangesToItem(item, analyticResult);
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
            if (mergeResult.OldItemAction == HCDataRepeaterItemMergeConflictResult.OldItemActionType.Delete)
            {
                await stream.Storage.DeleteItemAsync(existingItem.Id);
            }
            else if (mergeResult.OldItemAction == HCDataRepeaterItemMergeConflictResult.OldItemActionType.Update)
            {
                await stream.Storage.UpdateItemAsync(existingItem);
            }

            // Handle new item
            if (mergeResult.NewItemAction == HCDataRepeaterItemMergeConflictResult.NewItemActionType.Insert)
            {
                await stream.Storage.AddItemAsync(item, hint);
            }
            else if (mergeResult.NewItemAction == HCDataRepeaterItemMergeConflictResult.NewItemActionType.Ignore)
            {
                /* Ignored */
            }
        }

        /// <inheritdoc />
        public virtual async Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(string streamId, IHCDataRepeaterStreamItem item)
        {
            var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
            if (stream == null)
            {
                return HCDataRepeaterItemAnalysisResult.CreateError("Stream not found.");
            }

            item.Log ??= new();

            // Handle any exception
            HCDataRepeaterItemAnalysisResult result = null;
            try
            {
                result = await stream.AnalyzeItemAsync(item);
                if (result == null)
                {
                    return HCDataRepeaterItemAnalysisResult.CreateError("Analysis returned null.");
                }
            }
            catch (Exception ex)
            {
                item.LastRetryWasSuccessful = false;
                item.AddLogMessage($"Analysis was attempted. Failed with exception: {ex.Message}", MaxItemLogEntries);
                await stream.Storage.UpdateItemAsync(item);
                return HCDataRepeaterItemAnalysisResult.CreateError(ex);
            }

            // Update log
            var statusMessage = result.Message;
            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                statusMessage = "Executed successfully.";
            }
            item.AddLogMessage($"Analysis was attempted. Result: {statusMessage}", MaxItemLogEntries);

            // Apply AllowRetry and tag changes
            HCDataRepeaterUtils.ApplyChangesToItem(item, result);

            // Save changes
            await stream.Storage.UpdateItemAsync(item);

            return result;
        }

        /// <inheritdoc />
        public virtual async Task<HCDataRepeaterRetryResult> RetryItemAsync(string streamId, IHCDataRepeaterStreamItem item)
        {
            var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
            if (stream == null)
            {
                return HCDataRepeaterRetryResult.CreateError("Stream not found.");
            }

            item.LastRetriedAt = DateTimeOffset.Now;
            item.Log ??= new();

            // Handle any exception
            HCDataRepeaterRetryResult result = null;
            try
            {
                result = await stream.RetryItemAsync(item);
            }
            catch(Exception ex)
            {
                item.LastRetryWasSuccessful = false;
                item.AddLogMessage($"Retry was attempted. Failed with exception: {ex.Message}", MaxItemLogEntries);
                await stream.Storage.UpdateItemAsync(item);
                return HCDataRepeaterRetryResult.CreateError(ex);
            }

            // Update last success and message
            item.LastRetryWasSuccessful = result.Success;
            var statusMessage = result.Message;
            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                statusMessage = result.Success ? "Retry was successful" : "Retry failed.";
            }
            item.AddLogMessage($"Retry was attempted. Result: {statusMessage}", MaxItemLogEntries);

            // Apply AllowRetry and tag changes
            HCDataRepeaterUtils.ApplyChangesToItem(item, result);

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
        public virtual async Task<HCDataRepeaterStreamItemActionResult> PerformItemAction(string streamId, string actionId,
            IHCDataRepeaterStreamItem item, Dictionary<string, string> parameters)
        {
            var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
            if (stream == null)
            {
                return HCDataRepeaterStreamItemActionResult.CreateError($"Stream '{streamId}' not found.");
            }

            IHCDataRepeaterStreamItemAction action = stream.Actions?.FirstOrDefault(x => x?.GetType()?.FullName == actionId);
            if (action == null)
            {
                return HCDataRepeaterStreamItemActionResult.CreateError($"Action '{actionId}' not found.");
            }
            else if (action.AllowedOnItemsWithTags?.Any() == true
                && action.AllowedOnItemsWithTags.Any(t => item.Tags?.Contains(t) == true) == false)
            {
                return HCDataRepeaterStreamItemActionResult.CreateError($"Item '{item.ItemId}' does not allow for the action to be executed. Item is missing required tags.");
            }

            object parametersObject = action.ParametersType == null ? null : HCValueConversionUtils.ConvertInputModel(action.ParametersType, parameters);

            item.LastActionAt = DateTimeOffset.Now;
            item.Log ??= new();

            // Handle any exception
            HCDataRepeaterStreamItemActionResult result = null;
            try
            {
                result = await action.ExecuteActionAsync(item, parametersObject);
            }
            catch (Exception ex)
            {
                item.AddLogMessage($"Action '{action.DisplayName}' was executed. Failed with exception: {ex.Message}", MaxItemLogEntries);
                await stream.Storage.UpdateItemAsync(item);
                return HCDataRepeaterStreamItemActionResult.CreateError(ex);
            }

            // Update last success and message
            var statusMessage = result.Message;
            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                statusMessage = result.Success ? "Action was successful" : "Action failed.";
            }
            item.AddLogMessage($"Action '{action.DisplayName}' was executed. Result: {statusMessage}", MaxItemLogEntries);

            // Apply AllowRetry and tag changes
            HCDataRepeaterUtils.ApplyChangesToItem(item, result);

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
    }
}
