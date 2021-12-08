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

            return Task.FromResult(new HCGetDataExportStreamDefinitionsViewModel { 
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
                if (stream == null) return new HCDataExportQueryResponseViewModel { ErrorMessage = "Stream not found." };

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
        #endregion
    }
}
