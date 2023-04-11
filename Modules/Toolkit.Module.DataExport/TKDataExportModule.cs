using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Modules;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport
{
    /// <summary>
    /// Module for exporting data.
    /// </summary>
    public partial class TKDataExportModule : ToolkitModuleBase<TKDataExportModule.AccessOption>
    {
        /// <inheritdoc />
        public override List<string> AllCategories
            => Options?.Service?.GetStreams()
                ?.SelectMany(x => x.Categories ?? new())
                ?.Distinct()
                ?.ToList() ?? new List<string>();

        private TKDataExportModuleOptions Options { get; }
        private static readonly TKSimpleMemoryCache _allowedExports = new() { MaxCount = 1000 };
        private static readonly TKSimpleMemoryCache _exportStatuses = new() { MaxCount = 1000 };

        private class AllowedExportData
        {
            public Guid Key { get; set; }
            public TKDataExportQueryRequest Query { get; set; }
        }

        /// <summary>
        /// Module for exporting data.
        /// </summary>
        public TKDataExportModule(TKDataExportModuleOptions options)
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
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKDataExportModuleConfig();
        
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
        [ToolkitModuleMethod]
        public Task<TKGetDataExportStreamDefinitionsViewModel> GetStreamDefinitions(ToolkitModuleContext context)
        {
            var list = new List<TKDataExportStreamViewModel>();
            var streams = GetStreamsRequestCanAccess(context);
            foreach (var stream in streams)
            {
                var valueFormatters = (stream.ValueFormatters ?? Array.Empty<ITKDataExportValueFormatter>());
                var itemDef = Options.Service.GetStreamItemDefinition(stream, stream.ItemType);
                var showQueryInput = stream.SupportsQuery();
                var streamModel = new TKDataExportStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    Description = stream.StreamDescription,
                    GroupName = stream.StreamGroupName,
                    ItemDefinition = Create(itemDef),
                    ShowQueryInput = showQueryInput,
                    AllowAnyPropertyName = stream.AllowAnyPropertyName,
                    CustomParameterDefinitions = TKCustomPropertyAttribute.CreateInputConfigs(stream.CustomParametersType),
                    ValueFormatters = valueFormatters.Select(x => Create(x)).ToList()
                };
                streamModel.CustomParameterDefinitions = stream.PostprocessCustomParameterDefinitions(streamModel.CustomParameterDefinitions)
                    ?? streamModel.CustomParameterDefinitions;
                list.Add(streamModel);
            }

            var exporters = Options.Exporters?.Select(x => new TKDataExportExporterViewModel
            {
                Id = x?.GetType()?.FullName ?? "null",
                Name = x.DisplayName,
                Description = x.Description
            })?.ToList() ?? new List<TKDataExportExporterViewModel>();

            return Task.FromResult(new TKGetDataExportStreamDefinitionsViewModel
            {
                SupportsStorage = Options.PresetStorage != null,
                Streams = list,
                Exporters = exporters
            });
        }

        /// <summary>
        /// Get items paged.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<TKDataExportQueryResponseViewModel> QueryStreamPaged(ToolkitModuleContext context, TKDataExportQueryRequest model)
        {
            try
            {
                var stream = GetStream(context, model.StreamId);
                if (stream == null) return TKDataExportQueryResponseViewModel.CreateError("Stream not found.");

                var error = await PrepareQueryAsync(context, model, isExport: false);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    return TKDataExportQueryResponseViewModel.CreateError(error);
                }

                model.PageSize = Math.Min(model.PageSize, Options.MaxPageSize);

                var result = await Options.Service.QueryAsync(model);
                context?.AddAuditEvent($"Queried data export", subject: stream.StreamDisplayName);

                var additionalMembers = new List<TKDataExportStreamItemDefinitionMemberViewModel>();
                if (result.AdditionalMembers?.Any() == true)
                {
                    additionalMembers = result.AdditionalMembers.Select(x => new TKDataExportStreamItemDefinitionMemberViewModel
                    {
                        Name = x.Name,
                        TypeName = x.Type?.GetFriendlyTypeName() ?? string.Empty,
                        FormatterIds = Options.Service.GetValueFormatterIdsFor(stream, x.Type).ToArray()
                    }).ToList();
                }

                return new TKDataExportQueryResponseViewModel
                {
                    Success = true,
                    TotalCount = result.TotalCount,
                    Items = result.Items,
                    Note = result.Note,
                    AdditionalMembers = additionalMembers
                };
            }
            catch (Exception ex)
            {
                return new TKDataExportQueryResponseViewModel
                {
                    ErrorMessage = ex.Message,
                    ErrorDetails = TKExceptionUtils.GetFullExceptionDetails(ex)
                };
            }
        }

        /// <summary>
        /// Prepare export.
        /// </summary>
        [ToolkitModuleMethod(AccessOption.Export)]
        public async Task<Guid?> PrepareExport(ToolkitModuleContext context, TKDataExportQueryRequest model)
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
        [ToolkitModuleMethod(AccessOption.LoadPreset)]
        public async Task<List<TKDataExportStreamQueryPresetViewModel>> GetStreamQueryPresets(ToolkitModuleContext context, string streamId)
        {
            if (Options.PresetStorage == null || GetStream(context, streamId) == null) return new List<TKDataExportStreamQueryPresetViewModel>();

            var presets = await Options.PresetStorage.GetStreamQueryPresetsAsync(streamId);
            return presets.Select(x => Create(x)).ToList();
        }

        /// <summary>
        /// Delete a single query preset.
        /// </summary>
        [ToolkitModuleMethod(AccessOption.DeletePreset)]
        public async Task DeleteStreamQueryPresets(ToolkitModuleContext context, TKDataExportDeleteStreamQueryPresetsRequest request)
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
        [ToolkitModuleMethod(AccessOption.SavePreset)]
        public async Task<TKDataExportStreamQueryPresetViewModel> SaveStreamQueryPreset(ToolkitModuleContext context, TKDataExportSaveStreamQueryPresetRequest presetData)
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
        [ToolkitModuleAction(AccessOption.Export)]
        public object DEExport(ToolkitModuleContext context, string url)
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
                return new ToolkitStatusCodeOnlyResult(status);
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
                content = TKAsyncUtils.RunSync(() => CreateExportContentAsync(data, exporter, stream.ExportBatchSize));
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
            var fileName = $"{TKIOUtils.SanitizeFilename(stream.StreamDisplayName)}_{timestamp}{fileExtension}";

            context.AddAuditEvent("Data exported", streamName)
                .AddClientConnectionDetails(context)
                .AddDetail("File Name", fileName)
                .AddDetail("Filesize", $"{TKIOUtils.PrettifyFileSize(content.Length)}");

            _exportStatuses.Remove(data.Key.ToString());
            return ToolkitFileDownloadResult.CreateFromBytes(fileName, content);
        }
        #endregion

        #region Helpers
        private async Task<byte[]> CreateExportContentAsync(AllowedExportData data, ITKDataExportExporter exporter, int exportBatchSize)
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

        private async Task<string> PrepareQueryAsync(ToolkitModuleContext context, TKDataExportQueryRequest model, bool isExport)
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

        private IEnumerable<ITKDataExportStream> GetStreamsRequestCanAccess(ToolkitModuleContext context)
        {
            var streams = Options.Service?.GetStreams() ?? Enumerable.Empty<ITKDataExportStream>();
            return streams.Where(x =>
                (x.AllowedAccessRoles == null) || context.HasRoleAccessObj(x.AllowedAccessRoles)
                && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => x.Categories.Contains(c)))
            );
        }

        private ITKDataExportStream GetStream(ToolkitModuleContext context, string streamId)
        {
            var streams = GetStreamsRequestCanAccess(context);
            return streams.FirstOrDefault(x => x.GetType().FullName == streamId);
        }

        private static TKDataExportStreamItemDefinitionViewModel Create(TKDataExportStreamItemDefinition model)
        {
            return new TKDataExportStreamItemDefinitionViewModel
            {
                Name = model.Name,
                StreamId = model.StreamId,
                Members = model.Members?.Select(x => Create(x))?.ToList() ?? new()
            };
        }

        private static TKDataExportStreamItemDefinitionMemberViewModel Create(TKDataExportStreamItemDefinitionMember model)
        {
            return new TKDataExportStreamItemDefinitionMemberViewModel
            {
                Name = model.Name,
                TypeName = model.Type.GetFriendlyTypeName(),
                FormatterIds = model.FormatterIds
            };
        }

        private static TKDataExportStreamQueryPresetViewModel Create(TKDataExportStreamQueryPreset model)
        {
            if (model == null) return null;
            return new TKDataExportStreamQueryPresetViewModel
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

        private static TKDataExportStreamQueryPreset Create(TKDataExportStreamQueryPresetViewModel model, string streamId)
        {
            if (model == null) return null;
            return new TKDataExportStreamQueryPreset
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

        private static TKDataExportValueFormatterViewModel Create(ITKDataExportValueFormatter formatter)
        {
            return new TKDataExportValueFormatterViewModel
            {
                Id = formatter.GetType().FullName,
                Name = formatter.Name,
                Description = formatter.Description,
                SupportedTypes = (formatter.SupportedTypes?.Select(x => x.Name) ?? Enumerable.Empty<string>()).ToList(),
                CustomParameterDefinitions = TKCustomPropertyAttribute.CreateInputConfigs(formatter.CustomParametersType)
            };
        }
        #endregion
    }
}
