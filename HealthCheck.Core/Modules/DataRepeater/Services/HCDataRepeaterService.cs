using HealthCheck.Core.Modules.DataRepeater.Abstractions;
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
        public virtual async Task<HCDataRepeaterRetryResult> RetryItemAsync(string streamId, IHCDataRepeaterStreamItem item)
        {
            var stream = GetStreams()?.FirstOrDefault(x => x.GetType().FullName == streamId);
            if (stream == null)
            {
                return HCDataRepeaterRetryResult.CreateError("Stream or action not found.");
            }

            item.LastRetriedAt = DateTimeOffset.Now;
            item.Log ??= new();
            void log(string message)
            {
                item.Log.Add(new HCDataRepeaterSimpleLogEntry
                {
                    Timestamp = DateTimeOffset.Now,
                    Message = $"Retry was attempted. Result: {message}"
                });
                if (item.Log.Count > 10)
                {
                    item.Log = item.Log.Skip(item.Log.Count - 10).Take(10).ToList();
                }
            }

            // Handle any exception
            HCDataRepeaterRetryResult result = null;
            try
            {
                result = await stream.RetryItemAsync(item);
            }
            catch(Exception ex)
            {
                item.LastRetryWasSuccessful = false;
                await stream.Storage.UpdateItemAsync(item);
                log($"Failed with exception: {ex.Message}");
                return HCDataRepeaterRetryResult.CreateError(ex);
            }

            // Update last success and message
            item.LastRetryWasSuccessful = result.Success;
            var statusMessage = result.Message;
            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                statusMessage = result.Success ? "Retry was successful" : "Retry failed.";
            }
            log(statusMessage);

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
            void log(string message)
            {
                item.Log.Add(new HCDataRepeaterSimpleLogEntry
                {
                    Timestamp = DateTimeOffset.Now,
                    Message = $"Action '{action.DisplayName}' was executed. Result: {message}"
                });
                if (item.Log.Count > 10)
                {
                    item.Log = item.Log.Skip(item.Log.Count - 10).Take(10).ToList();
                }
            }

            // Handle any exception
            HCDataRepeaterStreamItemActionResult result = null;
            try
            {
                result = await action.ExecuteActionAsync(item, parametersObject);
            }
            catch (Exception ex)
            {
                log($"Failed with exception: {ex.Message}");
                await stream.Storage.UpdateItemAsync(item);
                return HCDataRepeaterStreamItemActionResult.CreateError(ex);
            }

            // Update last success and message
            var statusMessage = result.Message;
            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                statusMessage = result.Success ? "Action was successful" : "Action failed.";
            }
            log(statusMessage);

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
