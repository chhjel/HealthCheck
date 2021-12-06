using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.DataExport.Models;
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
        }

        #region Invokable methods
        /// <summary>
        /// Get stream details.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<List<HCDataExportStreamViewModel>> GetStreamDefinitions(HealthCheckModuleContext context)
        {
            var list = new List<HCDataExportStreamViewModel>();
            var streams = GetStreamsRequestCanAccess(context);
            foreach (var stream in streams)
            {
                var streamModel = new HCDataExportStreamViewModel
                {
                    Id = stream.GetType().FullName,
                    Name = stream.StreamDisplayName,
                    Description = stream.StreamDescription,
                    GroupName = stream.StreamGroupName
                };
                list.Add(streamModel);
            }
            return Task.FromResult(list);
        }

        /// <summary>
        /// Get items paged.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCDataExportQueryResponseViewModel> QueryStreamPaged(HealthCheckModuleContext context, HCDataExportQueryRequest model)
        {
            var stream = GetStream(context, model.StreamId);
            if (stream == null) return null;

            model.PageSize = Math.Min(model.PageSize, Options.MaxPageSize);
            var result = await Options.Service.QueryAsync(model);

            return new HCDataExportQueryResponseViewModel
            {
                TotalCount = result.TotalCount,
                Items = result.Items
            };
        }
        #endregion

        #region Helpers
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
        #endregion
    }
}
