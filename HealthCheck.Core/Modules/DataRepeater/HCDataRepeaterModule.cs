using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater
{
    /// <summary>
    /// Module for retrying data.
    /// </summary>
    public class HCDataRepeaterModule : HealthCheckModuleBase<HCDataRepeaterModule.AccessOption>
    {
        private HCDataRepeaterModuleOptions Options { get; }

        /// <summary>
        /// Module for retrying data.
        /// </summary>
        public HCDataRepeaterModule(HCDataRepeaterModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.Service == null) issues.Add("Options.Service must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCDataRepeaterModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,
            /// <summary>Allows retrying items.</summary>
            RetryItems = 1,
            /// <summary>Allows executing custom actions on items.</summary>
            ExecuteItemActions = 2,
        }

        #region Invokable methods
        /// <summary>
        /// Get stream details.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<HCGetDataRepeaterStreamDefinitionsViewModel> GetStreamDefinitions()
        {
            var model = new HCGetDataRepeaterStreamDefinitionsViewModel();
            var streams = Options.Service?.GetStreams() ?? Enumerable.Empty<IHCDataRepeaterStream>();
            foreach (var stream in streams)
            {
                var actions = stream.Actions ?? new List<IHCDataRepeaterStreamItemAction>();
                var streamModel = new HCDataRepeaterStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    ItemIdName = stream.ItemIdDisplayName,
                    GroupName = stream.StreamGroupName,
                    RetryActionName = stream.RetryActionName,
                    RetryDescription = stream.RetryDescription
                };
                model.Streams.Add(streamModel);
                foreach (var action in actions)
                {
                    streamModel.Actions.Add(new HCDataRepeaterStreamActionViewModel
                    {
                        Id = action.GetType().FullName,
                        Name = !string.IsNullOrWhiteSpace(action.DisplayName) ? action.DisplayName : action.GetType().Name,
                        Description = action.Description ?? "",
                        ExecuteButtonLabel = action.ExecuteButtonLabel,
                        AllowedOnItemsWithTags = action.AllowedOnItemsWithTags,
                        ParameterDefinitions = HCCustomPropertyAttribute.CreateInputConfigs(action.ParametersType)
                    });
                }
            }
            return Task.FromResult(model);
        }

        /// <summary>
        /// Get stream items paged.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCDataRepeaterStreamItemsPagedViewModel> GetStreamItemsPaged(HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            var stream = GetStream(model.StreamId);
            var result = await stream.Storage.GetItemsPagedAsync(model);
            var items = result.Items.Select(x => Create(x)).Where(x => x != null).ToList();
            
            return new HCDataRepeaterStreamItemsPagedViewModel
            {
                TotalCount = result.TotalCount,
                Items = items
            };
        }

        /// <summary>
        /// Get item details.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCDataRepeaterStreamItemDetailsViewModel> GetItemDetails(HCGetDataRepeaterItemDetailsRequest model)
        {
            var stream = GetStream(model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            var details = await stream.GetItemDetailsAsync(model.ItemId);
            return new HCDataRepeaterStreamItemDetailsViewModel
            {
                Description = details?.Description ?? "",
                Links = details?.Links ?? new(),
                Item = Create(item)
            };
        }

        /// <summary>
        /// Retry an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.RetryItems)]
        public async Task<HCDataRepeaterResultWithLogMessage<HCDataRepeaterRetryResult>> RetryItem(HCDataRepeaterRetryItemRequest model)
        {
            var stream = GetStream(model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            if (!item.AllowRetry)
            {
                return new HCDataRepeaterResultWithLogMessage<HCDataRepeaterRetryResult>
                {
                    Data = HCDataRepeaterRetryResult.CreateError("Item does not allow retry."),
                    LogMessage = null
                };
            }
            if (item.SerializedData != item.SerializedDataOverride)
            {
                item.SerializedDataOverride = model.SerializedDataOverride;
            }
            var data = await Options.Service.RetryItemAsync(model.StreamId, item);
            return new HCDataRepeaterResultWithLogMessage<HCDataRepeaterRetryResult>
            {
                Data = data,
                LogMessage = item.Log.Last()
            };
        }

        /// <summary>
        /// Perform an action on an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteItemActions)]
        public async Task<HCDataRepeaterResultWithLogMessage<HCDataRepeaterStreamItemActionResult>> PerformItemAction(HCDataRepeaterPerformItemActionRequest model)
        {
            var stream = GetStream(model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            var data = await Options.Service.PerformItemAction(model.StreamId, model.ActionId, item, model.Parameters);
            return new HCDataRepeaterResultWithLogMessage<HCDataRepeaterStreamItemActionResult>
            {
                Data = data,
                LogMessage = item.Log.Last()
            };
        }
        #endregion

        #region Helpers
        private HCDataRepeaterStreamItemViewModel Create(IHCDataRepeaterStreamItem item)
            => item == null ? null : new()
            {
                Id = item.Id,
                ItemId = item.ItemId,
                AllowRetry = item.AllowRetry,
                InsertedAt = item.InsertedAt,
                LastRetriedAt = item.LastRetriedAt,
                LastRetryWasSuccessful = item.LastRetryWasSuccessful,
                LastActionAt = item.LastActionAt,
                Tags = item.Tags?.ToList() ?? new List<string>(),
                InitialError = item.InitialError,
                Log = item.Log ?? new(),
                SerializedData = item.SerializedData ?? "",
                SerializedDataOverride = item.SerializedDataOverride ?? ""
            };

        private IHCDataRepeaterStream GetStream(string streamId)
        {
            var streams = Options.Service?.GetStreams();
            return streams.FirstOrDefault(x => x.GetType().FullName == streamId);
        }
        #endregion
    }
}
