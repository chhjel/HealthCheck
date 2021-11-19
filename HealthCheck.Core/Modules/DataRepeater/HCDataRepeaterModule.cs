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
        public async Task<HCGetDataRepeaterStreamDefinitionsViewModel> GetStreamDefinitions()
        {
            var model = new HCGetDataRepeaterStreamDefinitionsViewModel();
            var streams = (await Options.Service?.GetStreamsAsync()) ?? Enumerable.Empty<IHCDataRepeaterStream>();
            foreach (var stream in streams)
            {
                var actions = stream.Actions ?? new List<IHCDataRepeaterStreamItemAction>();
                var streamModel = new HCDataRepeaterStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    ItemIdName = stream.ItemIdDisplayName,
                    GroupName = stream.StreamGroupName
                };
                model.Streams.Add(streamModel);
                foreach (var action in actions)
                {
                    streamModel.Actions.Add(new HCDataRepeaterStreamActionViewModel
                    {
                        Id = action.GetType().FullName,
                        Name = !string.IsNullOrWhiteSpace(action.DisplayName) ? action.DisplayName : action.GetType().Name,
                        Description = action.Description ?? "",
                        ParameterDefinitions = HCCustomPropertyAttribute.CreateInputConfigs(action.ParametersType)
                    });
                }
            }
            return model;
        }

        /// <summary>
        /// Get stream items paged.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCDataRepeaterStreamItemsPagedViewModel> GetStreamItemsPaged(HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            var stream = await GetStreamAsync(model.StreamId);
            var result = await stream.GetItemsPagedAsync(model);
            var items = result.Items
                .Select(x => new HCDataRepeaterStreamItemViewModel
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    AllowRetry = x.AllowRetry,
                    InsertedAt = x.InsertedAt,
                    LastRetriedAt = x.LastRetriedAt,
                    LastRetryWasSuccessful = x.LastRetryWasSuccessful,
                    LastActionAt = x.LastActionAt,
                    LastActionWasSuccessful = x.LastActionWasSuccessful,
                    Tags = x.Tags?.ToList() ?? new List<string>(),
                    InitialError = x.InitialError,
                    Log = x.Log ?? new List<string>(),
                    SerializedData = x.SerializedData ?? ""
                })
                .ToList();
            
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
        public async Task<HCDataRepeaterStreamItemDetails> GetItemDetails(HCGetDataRepeaterItemDetailsRequest model)
        {
            var stream = await GetStreamAsync(model.StreamId);
            var details = await stream.GetItemDetailsAsync(model.ItemGuid, model.ItemId);
            return details;
        }

        /// <summary>
        /// Retry an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.RetryItems)]
        public async Task<HCDataRepeaterRetryResult> RetryItem(HCDataRepeaterRetryItemRequest model)
        {
            var stream = await GetStreamAsync(model.StreamId);
            var item = await stream.GetItemAsync(model.ItemGuid, model.ItemId);
            return await Options.Service.RetryItemAsync(model.StreamId, item);
        }

        /// <summary>
        /// Perform an action on an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteItemActions)]
        public async Task<HCDataRepeaterStreamItemActionResult> PerformItemAction(HCDataRepeaterPerformItemActionRequest model)
        {
            var stream = await GetStreamAsync(model.StreamId);
            var item = await stream.GetItemAsync(model.ItemGuid, model.ItemId);
            return await Options.Service.PerformItemAction(model.StreamId, model.ActionId, item, model.Parameters);
        }
        #endregion

        #region Helpers
        private async Task<IHCDataRepeaterStream> GetStreamAsync(string streamId)
        {
            var streams = await Options.Service.GetStreamsAsync();
            return streams.FirstOrDefault(x => x.GetType().FullName == streamId);
        }
        #endregion
    }
}
