﻿using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Util;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport
{
    /// <summary>
    /// Module for exporting data.
    /// </summary>
    public class HCDataExportModule : HealthCheckModuleBase<HCDataExportModule.AccessOption>
    {
        ///// <inheritdoc />
        //public override List<string> AllCategories
        //    => Options?.Service?.GetStreams()?.SelectMany(x => x.Categories)?.ToList() ?? new List<string>();

        private HCDataExportModuleOptions Options { get; }

        /// <summary>
        /// Module for exporting data.
        /// </summary>
        public HCDataExportModule(HCDataExportModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            //if (Options.Service == null) issues.Add("Options.Service must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCDataExportModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>Execute any custom queries.</summary>
            QueryCustom,

            /// <summary>Execute a preset query.</summary>
            QueryPreset,

            /// <summary>Save a new or overwrite an existing preset.</summary>
            SavePreset,

            /// <summary>Load existing presets.</summary>
            LoadPreset,

            /// <summary>Delete existing presets.</summary>
            DeletePreset,

            /// <summary>Export data.</summary>
            Export
        }

        #region Invokable methods
        /// <summary>
        /// Get stream details.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<HCGetDataExportStreamDefinitionsViewModel> GetStreamDefinitions(HealthCheckModuleContext context)
        {
            var list = new List<HCDataExportStreamViewModel>();
            var streams = GetStreamsRequestCanAccess(context);
            foreach (var stream in streams)
            {
                var itemDef = Options.Service.GetStreamItemDefinition(stream.GetType().FullName, stream.ItemType);
                var streamModel = new HCDataExportStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    Description = stream.StreamDescription,
                    GroupName = stream.StreamGroupName,
                    ItemDefinition = Create(itemDef)
                };
                list.Add(streamModel);
            }

            return Task.FromResult(new HCGetDataExportStreamDefinitionsViewModel
            {
                SupportsStorage = Options.PresetStorage != null,
                Streams = list
            });
        }

        /// <summary>
        /// Get items paged.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCDataExportQueryResponseViewModel> QueryStreamPaged(HealthCheckModuleContext context, HCDataExportQueryRequest model)
        {
            try
            {
                var stream = GetStream(context, model.StreamId);
                if (stream == null) return HCDataExportQueryResponseViewModel.CreateError("Stream not found.");

                var error = await PrepareQueryAsync(context, model, isExport: false);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    return HCDataExportQueryResponseViewModel.CreateError(error);
                }

                model.PageSize = Math.Min(model.PageSize, Options.MaxPageSize);

                var result = await Options.Service.QueryAsync(model);
                return new HCDataExportQueryResponseViewModel
                {
                    Success = true,
                    TotalCount = result.TotalCount,
                    Items = result.Items
                };
            }
            catch (Exception ex)
            {
                return new HCDataExportQueryResponseViewModel
                {
                    ErrorMessage = ex.Message,
                    ErrorDetails = ExceptionUtils.GetFullExceptionDetails(ex)
                };
            }
        }

        /// <summary>
        /// Get stream query presets.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.LoadPreset)]
        public async Task<List<HCDataExportStreamQueryPresetViewModel>> GetStreamQueryPresets(HealthCheckModuleContext context, string streamId)
        {
            if (Options.PresetStorage == null || GetStream(context, streamId) == null) return new List<HCDataExportStreamQueryPresetViewModel>();

            var presets = await Options.PresetStorage.GetStreamQueryPresetsAsync(streamId);
            return presets.Select(x => Create(x)).ToList();
        }

        /// <summary>
        /// Delete a single query preset.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.DeletePreset)]
        public async Task DeleteStreamQueryPresets(HealthCheckModuleContext context, HCDataExportDeleteStreamQueryPresetsRequest request)
        {
            if (Options.PresetStorage == null || GetStream(context, request.StreamId) == null) return;

            var preset = await Options.PresetStorage.GetStreamQueryPresetAsync(request.Id);
            if (preset.StreamId != request.StreamId) return;

            await Options.PresetStorage.DeleteStreamQueryPresetAsync(request.Id);
        }

        /// <summary>
        /// Save stream query preset.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.SavePreset)]
        public async Task<HCDataExportStreamQueryPresetViewModel> SaveStreamQueryPreset(HealthCheckModuleContext context, HCDataExportSaveStreamQueryPresetRequest presetData)
        {
            if (Options.PresetStorage == null || GetStream(context, presetData.StreamId) == null) return null;

            var existingPresets = await Options.PresetStorage.GetStreamQueryPresetsAsync(presetData.StreamId);
            var existingPreset = existingPresets.FirstOrDefault(x => x.Name == presetData.Preset.Name);
            if (existingPreset != null)
            {
                presetData.Preset.Id = existingPreset.Id;
            }

            var preset = await Options.PresetStorage.SaveStreamQueryPresetAsync(Create(presetData.Preset, presetData.StreamId));
            return Create(preset);
        }
        #endregion

        #region Helpers
        private async Task<string> PrepareQueryAsync(HealthCheckModuleContext context, HCDataExportQueryRequest model, bool isExport)
        {
            if (isExport && !context.HasAccess(AccessOption.Export))
            {
                return "You do not have access to export data.";
            }

            // Apply preset if specified and has access
            var usingPreset = false;
            if (model.PresetId != null
                && Options.PresetStorage != null
                && context.HasAccess(AccessOption.QueryPreset))
            {
                var preset = await Options.PresetStorage.GetStreamQueryPresetAsync(model.PresetId.Value);
                if (preset != null)
                {
                    usingPreset = true;
                    model.Query = preset.Query;
                    model.IncludedProperties = preset.IncludedProperties;
                    model.HeaderNameOverrides = preset.HeaderNameOverrides;
                }
            }

            // Verify query access
            if (!usingPreset && !context.HasAccess(AccessOption.QueryCustom))
            {
                return "You do not have access to execute custom queries.";
            }

            return null;
        }

        private IEnumerable<IHCDataExportStream> GetStreamsRequestCanAccess(HealthCheckModuleContext context)
        {
            var streams = Options.Service?.GetStreams() ?? Enumerable.Empty<IHCDataExportStream>();
            return streams.Where(x =>
                (x.AllowedAccessRoles == null) || context.HasRoleAccessObj(x.AllowedAccessRoles)
                && (x.Categories?.Any() != true || context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => x.Categories.Contains(c)))
            );
        }

        private IHCDataExportStream GetStream(HealthCheckModuleContext context, string streamId)
        {
            var streams = GetStreamsRequestCanAccess(context);
            return streams.FirstOrDefault(x => x.GetType().FullName == streamId);
        }

        private static HCDataExportStreamItemDefinitionViewModel Create(HCDataExportStreamItemDefinition model)
        {
            return new HCDataExportStreamItemDefinitionViewModel
            {
                Name = model.Name,
                StreamId = model.StreamId,
                Members = model.Members?.Select(x => Create(x))?.ToList() ?? new()
            };
        }

        private static HCDataExportStreamItemDefinitionMemberViewModel Create(HCDataExportStreamItemDefinitionMember model)
        {
            return new HCDataExportStreamItemDefinitionMemberViewModel
            {
                Name = model.Name,
                TypeName = model.Type.Name
            };
        }

        private static HCDataExportStreamQueryPresetViewModel Create(HCDataExportStreamQueryPreset model)
        {
            if (model == null) return null;
            return new HCDataExportStreamQueryPresetViewModel
            {
                Name = model.Name,
                Description = model.Description,
                IncludedProperties = model.IncludedProperties,
                Query = model.Query,
                HeaderNameOverrides = model.HeaderNameOverrides
            };
        }

        private static HCDataExportStreamQueryPreset Create(HCDataExportStreamQueryPresetViewModel model, string streamId)
        {
            if (model == null) return null;
            return new HCDataExportStreamQueryPreset
            {
                Id = model.Id ?? Guid.Empty,
                StreamId = streamId,
                Name = model.Name,
                Description = model.Description,
                IncludedProperties = model.IncludedProperties,
                Query = model.Query,
                HeaderNameOverrides = model.HeaderNameOverrides
            };
        }
        #endregion
    }
}
