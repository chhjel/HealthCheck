using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport
{
    /// <summary>
    /// Module for exporting data.
    /// </summary>
    public class HCDataExportModule : HealthCheckModuleBase<HCDataExportModule.AccessOption>
    {
        /// <inheritdoc />
        public override List<string> AllCategories
            => Options?.Service?.GetStreams()?.SelectMany(x => x.Categories)?.ToList() ?? new List<string>();

        private HCDataExportModuleOptions Options { get; }
        private static readonly SimpleMemoryCache _allowedExports = new() { MaxCount = 1000 };
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
                    ErrorDetails = ExceptionUtils.GetFullExceptionDetails(ex)
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
            if (!match.Success)
            {
                return null;
            }

            // Parse url
            var keyFromUrl = match.Groups["key"].Value.Trim().ToLower();
            var data = _allowedExports.GetValue<AllowedExportData>(keyFromUrl, null);
            if (data == null)
            {
                return CreateExportErrorHtml("Data to export not found.");
            }
            _allowedExports.ClearKey(keyFromUrl);

            // Validate
            var stream = GetStream(context, data.Query.StreamId);
            if (stream == null) return CreateExportErrorHtml("Export data stream not found.");

            // Bake file
            string content = null;
            try
            {
                content = AsyncUtils.RunSync(() => CreateExportContentAsync(data, stream.ExportBatchSize));
            }
            catch (Exception ex)
            {
                return CreateExportErrorHtml(ex.Message);
            }

            // Store audit data
            var streamName = data.Query.StreamId;
            if (streamName.Contains("."))
            {
                streamName = streamName.Substring(streamName.LastIndexOf(".") + 1);
            }

            // Create filename
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileExtension = ".txt";
            var fileName = $"{IOUtils.SanitizeFilename(stream.StreamDisplayName)}_{timestamp}.{fileExtension}";

            context.AddAuditEvent("Data exported", streamName)
                .AddClientConnectionDetails(context)
                .AddDetail("File Name", fileName)
                .AddDetail("Filesize", $"{Encoding.Default.GetBytes(content)} bytes");
                //.AddDetail("Query", );

            return HealthCheckFileDownloadResult.CreateFromString(fileName, content);
        }
        #endregion

        #region Helpers
        private async Task<string> CreateExportContentAsync(AllowedExportData data, int exportBatchSize)
        {
            var model = data.Query;
            model.PageIndex = 0;
            model.PageSize = exportBatchSize;

            // todo: specify from frontend what exporter type to use
            HCDataExportExporter exporter = new HCDataExportExporterCSV();

            var headers = data.Query.IncludedProperties;
            var headerNameOverrides = data.Query.HeaderNameOverrides ?? new();
            var resolvedHeaders = new List<string>();
            foreach (var header in headers)
            {
                var name = headerNameOverrides.ContainsKey(header) ? headerNameOverrides[header] : header;
                resolvedHeaders.Add(name);
            }
            exporter.SetHeaders(resolvedHeaders);

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
                    exporter.AppendItem(item, headers);
                }
            }

            return exporter.GetContents();
        }

        /// <summary>
        /// Outputs CSV.
        /// </summary>
        public class HCDataExportExporterCSV : HCDataExportExporter
        {
            /// <inheritdoc />
            public string DisplayName => "CSV";

            /// <inheritdoc />
            public string FileExtension => ".txt";

            /// <summary>
            /// Delimiter to separate values with.
            /// </summary>
            public string Delimiter { get; set; } = ";";

            private readonly StringBuilder _builder = new();

            /// <inheritdoc />
            public void SetHeaders(List<string> headers)
            {
                _builder.AppendLine(CreateLine(headers));
            }

            /// <inheritdoc />
            public void AppendItem(Dictionary<string, object> item, List<string> order)
            {
                var parts = order.Select(x => item[x]?.ToString());
                var line = CreateLine(parts);
                _builder.AppendLine(line);
            }

            /// <inheritdoc />
            public string GetContents() => _builder.ToString();

            private string CreateLine(IEnumerable<string> parts)
                => string.Join(Delimiter, parts.Select(x => PrepareValue(x)));

            private string PrepareValue(string value)
            {
                if (value == null) return "";
                if (value.Contains(Delimiter))
                {
                    var escaped = value.Replace("\"", "\"\"");
                    return $"\"{escaped}\"";
                }
                return value;
            }
        }

        /// <summary>
        /// Type of output data to be exported.
        /// </summary>
        public interface HCDataExportExporter
        {
            /// <summary>
            /// Name shown in the UI.
            /// </summary>
            string DisplayName { get; }

            /// <summary>
            /// Extension used for filename.
            /// </summary>
            string FileExtension { get; }

            /// <summary>
            /// Sets the headers for this data.
            /// </summary>
            void SetHeaders(List<string> headers);

            /// <summary>
            /// Append a new item to be exported.
            /// </summary>
            void AppendItem(Dictionary<string, object> item, List<string> order);

            /// <summary>
            /// Get current contents.
            /// </summary>
            string GetContents();
        }

        private string CreateExportErrorHtml(string error)
        {
            var Q = "\"";
            var noIndexMeta = $"<meta name={Q}robots{Q} content={Q}noindex{Q}>";

            return $@"
<!doctype html>
<html>
<head>
    <title>Export failed</title>
    {noIndexMeta}
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
</head>

<body>
    <div>
        <p>{error}</p>
    </div>
</body>
</html>";
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
                Id = model.Id,
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
