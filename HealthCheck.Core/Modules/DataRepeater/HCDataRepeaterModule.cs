using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Util;
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
        /// <inheritdoc />
        public override List<string> AllCategories
        {
            get
            {
                var cats = new HashSet<string>();
                foreach(var stream in Options?.Service?.GetStreams() ?? Enumerable.Empty<IHCDataRepeaterStream>())
                {
                    if (stream.Categories?.Any() == true)
                    {
                        stream.Categories.ForEach(x => cats.Add(x));
                    }
                    if (stream.Actions?.Any() == true)
                    {
                        foreach (var actionCat in stream.Actions.SelectMany(x => x.Categories))
                        {
                            cats.Add(actionCat);
                        }
                    }
                    if (stream.BatchActions?.Any() == true)
                    {
                        foreach (var actionCat in stream.BatchActions.SelectMany(x => x.Categories))
                        {
                            cats.Add(actionCat);
                        }
                    }
                }
                return cats.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
        }

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
            /// <summary>Allows executing analysis on items manually.</summary>
            ManualAnalysis = 1,
            /// <summary>Allows retrying items.</summary>
            RetryItems = 2,
            /// <summary>Allows executing custom actions on items.</summary>
            ExecuteItemActions = 4,
            /// <summary>Allows executing custom batch actions on items.</summary>
            ExecuteBatchActions = 8
        }

        #region Invokable methods
        /// <summary>
        /// Get stream details.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<HCGetDataRepeaterStreamDefinitionsViewModel> GetStreamDefinitions(HealthCheckModuleContext context)
        {
            var model = new HCGetDataRepeaterStreamDefinitionsViewModel();
            var streams = GetStreamsRequestCanAccess(context);
            foreach (var stream in streams)
            {
                var streamModel = new HCDataRepeaterStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    Description = stream.StreamDescription,
                    StreamItemsName = stream.StreamItemsName,
                    ItemIdName = stream.ItemIdDisplayName,
                    GroupName = stream.StreamGroupName,
                    RetryActionName = stream.RetryActionName,
                    RetryDescription = stream.RetryDescription,
                    ManualAnalyzeEnabled = stream.ManualAnalyzeEnabled,
                    AnalyzeActionName = stream.AnalyzeActionName,
                    FilterableTags = stream.FilterableTags ?? new(),
                    InitiallySelectedTags = stream.InitiallySelectedTags ?? new()
                };
                model.Streams.Add(streamModel);

                var actions = (stream.Actions ?? new List<IHCDataRepeaterStreamItemAction>())
                    .Where(x => RequestCanAccessAction(context, x));
                foreach (var action in actions)
                {
                    streamModel.Actions.Add(new HCDataRepeaterStreamActionViewModel
                    {
                        Id = action.GetType().FullName,
                        Name = !string.IsNullOrWhiteSpace(action.DisplayName) ? action.DisplayName : action.GetType().Name,
                        Description = action.Description ?? "",
                        ExecuteButtonLabel = action.ExecuteButtonLabel,
                        ParameterDefinitions = HCCustomPropertyAttribute.CreateInputConfigs(action.ParametersType)
                    });
                }

                var batchActions = (stream.BatchActions ?? new List<IHCDataRepeaterStreamItemBatchAction>())
                    .Where(x => RequestCanAccessBatchAction(context, x));
                foreach (var action in batchActions)
                {
                    streamModel.BatchActions.Add(new HCDataRepeaterStreamBatchActionViewModel
                    {
                        Id = action.GetType().FullName,
                        Name = !string.IsNullOrWhiteSpace(action.DisplayName) ? action.DisplayName : action.GetType().Name,
                        Description = action.Description ?? "",
                        ExecuteButtonLabel = action.ExecuteButtonLabel,
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
        public async Task<HCDataRepeaterStreamItemsPagedViewModel> GetStreamItemsPaged(HealthCheckModuleContext context, HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            var stream = GetStream(context, model.StreamId);
            var result = await stream.Storage.GetItemsPagedAsync(model);
            var items = result.Items.Select(x => Create(x, stream, checkActions: false)).Where(x => x != null).ToList();
            
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
        public async Task<HCDataRepeaterStreamItemDetailsViewModel> GetItemDetails(HealthCheckModuleContext context, HCGetDataRepeaterItemDetailsRequest model)
        {
            var stream = GetStream(context, model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            var details = await stream.GetItemDetailsAsync(item);
            return new HCDataRepeaterStreamItemDetailsViewModel
            {
                Description = details?.DescriptionHtml ?? "",
                Links = details?.Links ?? new(),
                Item = Create(item, stream, checkActions: true)
            };
        }

        /// <summary>
        /// Analyses an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ManualAnalysis)]
        public async Task<HCDataRepeaterResultWithItem<HCDataRepeaterItemAnalysisResult>> AnalyseItem(HealthCheckModuleContext context, HCDataRepeaterAnalyzeItemRequest model)
        {
            var stream = GetStream(context, model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            var data = await Options.Service.AnalyzeItemAsync(model.StreamId, item);
            item = await stream.Storage.GetItemAsync(model.ItemId);

            context.AddAuditEvent(action: "Manual analysis", subject: item?.ItemId ?? "<null>")
                .AddDetail("Stream", stream?.GetType()?.Name ?? "<null>");

            return new HCDataRepeaterResultWithItem<HCDataRepeaterItemAnalysisResult>
            {
                Item = Create(item, stream, checkActions: true),
                Data = data
            };
        }

        /// <summary>
        /// Retry an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.RetryItems)]
        public async Task<HCDataRepeaterResultWithItem<HCDataRepeaterRetryResult>> RetryItem(HealthCheckModuleContext context, HCDataRepeaterRetryItemRequest model)
        {
            var stream = GetStream(context, model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            if (!item.AllowRetry)
            {
                return new HCDataRepeaterResultWithItem<HCDataRepeaterRetryResult>
                {
                    Item = Create(item, stream, checkActions: true),
                    Data = HCDataRepeaterRetryResult.CreateError("Item does not allow retry.")
                };
            }

            // Update serialized data from frontend
            item.SerializedData = model.SerializedDataOverride;

            var data = await Options.Service.RetryItemAsync(model.StreamId, item);
            item = await stream.Storage.GetItemAsync(model.ItemId);

            context.AddAuditEvent(action: "Retry item", subject: item?.ItemId ?? "<null>")
                .AddDetail("Stream", stream?.GetType()?.Name ?? "<null>")
                .AddDetail("Success", data?.Success.ToString() ?? "<null>");

            return new HCDataRepeaterResultWithItem<HCDataRepeaterRetryResult>
            {
                Item = Create(item, stream, checkActions: true),
                Data = data
            };
        }

        /// <summary>
        /// Perform an action on an item.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteItemActions)]
        public async Task<HCDataRepeaterResultWithItem<HCDataRepeaterStreamItemActionResult>> PerformItemAction(HealthCheckModuleContext context, HCDataRepeaterPerformItemActionRequest model)
        {
            if (!RequestCanAccessAction(context, model.StreamId, model.ActionId))
            {
                return new HCDataRepeaterResultWithItem<HCDataRepeaterStreamItemActionResult>
                {
                    Data = HCDataRepeaterStreamItemActionResult.CreateError("Request does not have access to the given action.")
                };
            }

            var stream = GetStream(context, model.StreamId);
            var item = await stream.Storage.GetItemAsync(model.ItemId);
            var data = await Options.Service.PerformItemAction(model.StreamId, model.ActionId, item, model.Parameters);
            item = await stream.Storage.GetItemAsync(model.ItemId);

            context.AddAuditEvent(action: "Execute action", subject: item?.ItemId ?? "<null>")
                .AddDetail("Stream", stream?.GetType()?.Name ?? "<null>")
                .AddDetail("Action", model?.ActionId ?? "<null>")
                .AddDetail("Success", data?.Success.ToString() ?? "<null>");

            return new HCDataRepeaterResultWithItem<HCDataRepeaterStreamItemActionResult>
            {
                Item = Create(item, stream, checkActions: true),
                Data = data,
            };
        }

        /// <summary>
        /// Perform a batch action on all the items in the given stream.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteBatchActions)]
        public async Task<HCDataRepeaterStreamBatchActionResult> PerformBatchAction(HealthCheckModuleContext context, HCDataRepeaterPerformBatchActionRequest model)
        {
            if (!RequestCanAccessBatchAction(context, model.StreamId, model.ActionId))
            {
                return HCDataRepeaterStreamBatchActionResult.CreateError("Request does not have access to the given batch action.");
            }

            var stream = GetStream(context, model.StreamId);
            var result = await Options.Service.PerformItemBatchAction(model.StreamId, model.ActionId, model.Parameters);

            context.AddAuditEvent(action: "Execute batch action", subject: model.ActionId ?? "<null>")
                .AddDetail("Stream", stream?.GetType()?.Name ?? "<null>")
                .AddDetail("Batch action", model?.ActionId ?? "<null>")
                .AddDetail("Result", result.ToString() ?? "<null>");

            return result;
        }
        #endregion

        #region Helpers
        private HCDataRepeaterStreamItemViewModel Create(IHCDataRepeaterStreamItem item, IHCDataRepeaterStream stream, bool checkActions)
        {
            if (item == null) return null;
            var model = new HCDataRepeaterStreamItemViewModel()
            {
                Id = item.Id,
                ItemId = item.ItemId,
                Summary = item.Summary,
                ForcedStatus = item.ForcedStatus,
                AllowRetry = item.AllowRetry,
                InsertedAt = item.InsertedAt,
                LastRetriedAt = item.LastRetriedAt,
                LastUpdatedAt = item.LastUpdatedAt,
                LastRetryWasSuccessful = item.LastRetryWasSuccessful,
                LastActionAt = item.LastActionAt,
                ExpiresAt = item.ExpirationTime,
                Tags = item.Tags?.ToList() ?? new List<string>(),
                FirstError = item.FirstError,
                Error = item.Error,
                FirstErrorAt = item.FirstErrorAt,
                LastErrorAt = item.LastErrorAt,
                Log = item.Log ?? new(),
                SerializedData = item.SerializedData ?? "",
                FirstSerializedData = item.FirstSerializedData ?? ""
            };

            if (checkActions && stream?.Actions?.Any() == true)
            {
                foreach (var action in stream.Actions)
                {
                    var result = HCAsyncUtils.RunSync(() => action.ActionIsAllowedForAsync(item));
                    model.ActionValidationResults.Add(new HCDataRepeaterStreamItemActionAllowedViewModel
                    {
                        ActionId = action?.GetType()?.FullName ?? "-null-",
                        Allowed = result.Allowed,
                        Reason = result.Reason
                    });
                }
            }

            return model;
        }

        private bool RequestCanAccessAction(HealthCheckModuleContext context, string streamId, string actionId)
        {
            var stream = GetStreamsRequestCanAccess(context).FirstOrDefault(x => x.GetType().FullName == streamId);
            var action = stream.Actions?.FirstOrDefault(x => x?.GetType()?.FullName == actionId);
            return RequestCanAccessAction(context, action);
        }

        private bool RequestCanAccessAction(HealthCheckModuleContext context, IHCDataRepeaterStreamItemAction action)
        {
            return action != null
                && context.HasAccess(AccessOption.ExecuteItemActions)
                && (action.AllowedAccessRoles == null || context.HasRoleAccessObj(action.AllowedAccessRoles))
                && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => action.Categories.Contains(c)));
        }

        private bool RequestCanAccessBatchAction(HealthCheckModuleContext context, string streamId, string actionId)
        {
            var stream = GetStreamsRequestCanAccess(context).FirstOrDefault(x => x.GetType().FullName == streamId);
            var action = stream.BatchActions?.FirstOrDefault(x => x?.GetType()?.FullName == actionId);
            return RequestCanAccessBatchAction(context, action);
        }

        private bool RequestCanAccessBatchAction(HealthCheckModuleContext context, IHCDataRepeaterStreamItemBatchAction batchAction)
        {
            return batchAction != null
                && context.HasAccess(AccessOption.ExecuteBatchActions)
                && (batchAction.AllowedAccessRoles == null || context.HasRoleAccessObj(batchAction.AllowedAccessRoles))
                && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => batchAction.Categories.Contains(c)));
        }

        private IEnumerable<IHCDataRepeaterStream> GetStreamsRequestCanAccess(HealthCheckModuleContext context)
        {
            var streams = Options.Service?.GetStreams() ?? Enumerable.Empty<IHCDataRepeaterStream>();
            return streams.Where(x =>
                (x.AllowedAccessRoles == null) || context.HasRoleAccessObj(x.AllowedAccessRoles)
                && (x.Categories?.Any() != true || context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => x.Categories.Contains(c)))
            );
        }

        private IHCDataRepeaterStream GetStream(HealthCheckModuleContext context, string streamId)
        {
            var streams = GetStreamsRequestCanAccess(context);
            return streams.FirstOrDefault(x => x.GetType().FullName == streamId);
        }
        #endregion
    }
}
