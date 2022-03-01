using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport
{
    /// <summary>
    /// Module for exporting data.
    /// </summary>
    public partial class HCDataExportModule : HealthCheckModuleBase<HCDataExportModule.AccessOption>
    {
        /// <inheritdoc />
        public override List<string> AllCategories
            => Options?.Service?.GetStreams()
                ?.SelectMany(x => x.Categories ?? new())
                ?.Distinct()
                ?.ToList() ?? new List<string>();

        private HCDataExportModuleOptions Options { get; }
        private static readonly HCSimpleMemoryCache _allowedExports = new() { MaxCount = 1000 };
        private static readonly HCSimpleMemoryCache _exportStatuses = new() { MaxCount = 1000 };

        private class AllowedExportData
        {
            public Guid Key { get; set; }
            public HCDataExportQueryRequest Query { get; set; }
        }

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
            if (Options.Service == null) issues.Add("Options.Service must be set.");
            if (Options.Exporters?.Any() != true) issues.Add("Options.Exporters must contain at least 1 exporter.");
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
            QueryCustom = 1,

            /// <summary>Execute a preset query.</summary>
            QueryPreset = 2,

            /// <summary>Save a new or overwrite an existing preset.</summary>
            SavePreset = 4,

            /// <summary>Load existing presets.</summary>
            LoadPreset = 8,

            /// <summary>Delete existing presets.</summary>
            DeletePreset = 16,

            /// <summary>Export data.</summary>
            Export = 32
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
                var valueFormatters = (stream.ValueFormatters ?? Array.Empty<IHCDataExportValueFormatter>());
                var itemDef = Options.Service.GetStreamItemDefinition(stream.GetType().FullName, stream.ItemType, valueFormatters);
                var showQueryInput = stream.SupportsQuery;
                var streamModel = new HCDataExportStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    Description = stream.StreamDescription,
                    GroupName = stream.StreamGroupName,
                    ItemDefinition = Create(itemDef),
                    ShowQueryInput = showQueryInput,
                    CustomParameterDefinitions = HCCustomPropertyAttribute.CreateInputConfigs(stream.CustomParametersType),
                    ValueFormatters = valueFormatters.Select(x => Create(x)).ToList()
                };
                list.Add(streamModel);
            }

            var exporters = Options.Exporters?.Select(x => new HCDataExportExporterViewModel
            {
                Id = x?.GetType()?.FullName ?? "null",
                Name = x.DisplayName,
                Description = x.Description
            })?.ToList() ?? new List<HCDataExportExporterViewModel>();

            return Task.FromResult(new HCGetDataExportStreamDefinitionsViewModel
            {
                SupportsStorage = Options.PresetStorage != null,
                Streams = list,
                Exporters = exporters
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
                context?.AddAuditEvent($"Queried data export", subject: stream.StreamDisplayName);
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
                    ErrorDetails = HCExceptionUtils.GetFullExceptionDetails(ex)
                };
            }
        }

        /// <summary>
        /// Prepare export.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.Export)]
        public async Task<Guid?> PrepareExport(HealthCheckModuleContext context, HCDataExportQueryRequest model)
        {
            var stream = GetStream(context, model.StreamId);
            if (stream == null) return null;

            var error = await PrepareQueryAsync(context, model, isExport: true);
            if (!string.IsNullOrWhiteSpace(error))
            {
                return null;
            }

            var data = new AllowedExportData
            {
                Key = Guid.NewGuid(),
                Query = model
            };
            _allowedExports.Set(data.Key.ToString(), data, TimeSpan.FromMinutes(1));
            _exportStatuses.Set(data.Key.ToString(), HttpStatusCode.Found, TimeSpan.FromMinutes(10));
            return data.Key;
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

            context?.AddAuditEvent($"Deleted data export query preset", subject: preset.Name);
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
            context?.AddAuditEvent($"Saved data export query preset", subject: presetData.Preset.Name);
            return Create(preset);
        }
        #endregion

        #region Actions
        private static readonly Regex DownloadExportFileUrlRegex
            = new(@"^/DEExport/(?<key>[\w-]+)/?", RegexOptions.IgnoreCase);

        /// <summary>
        /// Export some data into a file.
        /// </summary>
        [HealthCheckModuleAction(AccessOption.Export)]
        public object DEExport(HealthCheckModuleContext context, string url)
        {
            var match = DownloadExportFileUrlRegex.Match(url);
            if (!match.Success) return null;

            // Parse url
            var keyFromUrl = match.Groups["key"].Value.Trim().ToLower();
            var isHeadStatusRequest = context.Request.Method == "HEAD"
                && (url?.EndsWith("?status=1") == true || url?.EndsWith("&status=1") == true);
            if (isHeadStatusRequest)
            {
                var status = _exportStatuses.GetValue(keyFromUrl, HttpStatusCode.NotFound);
                return new HealthCheckStatusCodeOnlyResult(status);
            }
            // No other head requests used
            else if (context.Request.Method == "HEAD") return null;

            // Validate
            var data = _allowedExports.GetValue<AllowedExportData>(keyFromUrl, null);
            if (data == null)
            {
                return CreateExportErrorHtml(context, "Data to export not found.");
            }

            // Show loading page first that redirects to this action again without loader
            var showLoadingDownloadPage = url?.EndsWith("?dl=1") == false;
            if (showLoadingDownloadPage)
            {
                url = url.Substring("/DEExport/".Length);
                url += url.Contains('?') ? "&dl=1" : "?dl=1";
                return CreateExportLoadingDownloadHtml(context, $"{url}?dl=1");
            }

            // Invalidate key
            _allowedExports.ClearKey(keyFromUrl, allowDistribute: false);

            // Validate
            var stream = GetStream(context, data.Query.StreamId);
            if (stream == null) return CreateExportErrorHtml(context, "Export data stream not found.");
            var exporter = Options.Exporters?.FirstOrDefault(x => x.GetType().FullName == data.Query.ExporterId);
            if (exporter == null) return CreateExportErrorHtml(context, "Export method not found.");

            // Bake file
            byte[] content = null;
            try
            {
                content = HCAsyncUtils.RunSync(() => CreateExportContentAsync(data, exporter, stream.ExportBatchSize));
            }
            catch (Exception ex)
            {
                return CreateExportErrorHtml(context, ex.Message);
            }

            // Store audit data
            var streamName = data.Query.StreamId;
            if (streamName.Contains('.'))
            {
                streamName = streamName.Substring(streamName.LastIndexOf(".") + 1);
            }

            // Create filename
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileExtension = exporter.FileExtension ?? ".txt";
            var fileName = $"{HCIOUtils.SanitizeFilename(stream.StreamDisplayName)}_{timestamp}{fileExtension}";

            context.AddAuditEvent("Data exported", streamName)
                .AddClientConnectionDetails(context)
                .AddDetail("File Name", fileName)
                .AddDetail("Filesize", $"{HCIOUtils.PrettifyFileSize(content.Length)}");

            _exportStatuses.Remove(data.Key.ToString());
            return HealthCheckFileDownloadResult.CreateFromBytes(fileName, content);
        }
        #endregion

        #region Helpers
        private async Task<byte[]> CreateExportContentAsync(AllowedExportData data, IHCDataExportExporter exporter, int exportBatchSize)
        {
            var model = data.Query;
            model.PageIndex = 0;
            model.PageSize = exportBatchSize;

            var headerOrder = data.Query.IncludedProperties;
            var headerNameOverrides = data.Query.HeaderNameOverrides ?? new();
            var resolvedHeaders = new Dictionary<string, string>();
            foreach (var header in headerOrder)
            {
                var name = headerNameOverrides.ContainsKey(header) ? headerNameOverrides[header] : header;
                resolvedHeaders[header] = name;
            }
            exporter.SetHeaders(resolvedHeaders, headerOrder);

            var taken = 0;
            var totalCount = 1;
            while (taken < totalCount)
            {
                var result = await Options.Service.QueryAsync(model);
                totalCount = result.TotalCount;
                model.PageIndex++;

                foreach (var item in result.Items.OfType<Dictionary<string, object>>())
                {
                    taken++;
                    exporter.AppendItem(item, resolvedHeaders, headerOrder);
                }
            }

            return exporter.GetContents();
        }

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
                && (isExport || context.HasAccess(AccessOption.QueryPreset)))
            {
                var preset = await Options.PresetStorage.GetStreamQueryPresetAsync(model.PresetId.Value);
                if (preset != null)
                {
                    usingPreset = true;
                    model.Query = preset.Query;
                    model.IncludedProperties = preset.IncludedProperties;
                    model.HeaderNameOverrides = preset.HeaderNameOverrides;
                    model.CustomParameters = preset.CustomParameters;
                    model.ValueFormatterConfigs = preset.ValueFormatterConfigs;
                    model.CustomColumns = preset.CustomColumns;
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
                && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => x.Categories.Contains(c)))
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
                TypeName = model.Type.GetFriendlyTypeName(),
                FormatterIds = model.FormatterIds
            };
        }

        private static HCDataExportStreamQueryPresetViewModel Create(HCDataExportStreamQueryPreset model)
        {
            if (model == null) return null;
            return new HCDataExportStreamQueryPresetViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                IncludedProperties = model.IncludedProperties,
                Query = model.Query,
                HeaderNameOverrides = model.HeaderNameOverrides,
                CustomParameters = model.CustomParameters,
                ValueFormatterConfigs = model.ValueFormatterConfigs,
                CustomColumns = model.CustomColumns
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
                HeaderNameOverrides = model.HeaderNameOverrides,
                CustomParameters = model.CustomParameters,
                ValueFormatterConfigs = model.ValueFormatterConfigs,
                CustomColumns = model.CustomColumns
            };
        }

        private static HCDataExportValueFormatterViewModel Create(IHCDataExportValueFormatter formatter)
        {
            return new HCDataExportValueFormatterViewModel
            {
                Id = formatter.GetType().FullName,
                Name = formatter.Name,
                Description = formatter.Description,
                SupportedTypes = (formatter.SupportedTypes?.Select(x => x.Name) ?? Enumerable.Empty<string>()).ToList(),
                CustomParameterDefinitions = HCCustomPropertyAttribute.CreateInputConfigs(formatter.CustomParametersType)
            };
        }
        #endregion
    }
}
