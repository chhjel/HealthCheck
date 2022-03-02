using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Module for viewing custom data.
    /// </summary>
    public class HCDataflowModule<TAccessRole> : HealthCheckModuleBase<HCDataflowModule<TAccessRole>.AccessOption>
    {
        private HCDataflowModuleOptions<TAccessRole> Options { get; }

        /// <summary>
        /// Module for viewing custom data.
        /// </summary>
        public HCDataflowModule(HCDataflowModuleOptions<TAccessRole> options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.DataflowService == null) issues.Add("Options.DataflowService must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCDataflowModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,
            /// <summary>Allows fetching data from streams.</summary>
            FetchStream = 1,
            /// <summary>Allows performing unified searches.</summary>
            UnifiedSearch = 2
        }

        #region Invokable methods
        /// <summary>
        /// Get viewmodel for dataflow entries result.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<IEnumerable<IDataflowEntry>> GetDataflowStreamEntries(HealthCheckModuleContext context, GetDataflowStreamEntriesFilter model)
        {
            if (Options.DataflowService == null || !context.HasAccess(AccessOption.FetchStream))
                return Enumerable.Empty<IDataflowEntry>();

            var metadatas = GetDataflowStreamsMetadata(context);
            if (!metadatas.Any())
                return Enumerable.Empty<IDataflowEntry>();

            var stream = metadatas.FirstOrDefault(x => x.Id == model.StreamId);
            if (stream != null)
            {
                context.AddAuditEvent(action: "Dataflow stream fetched", subject: stream?.Name)
                    .AddDetail("Stream id", stream?.Id);
            }

            model.StreamFilter ??= new DataflowStreamFilter();
            model.StreamFilter.PropertyFilters ??= new Dictionary<string, string>();
            return await Options.DataflowService.GetEntries(model.StreamId, model.StreamFilter);
        }

        /// <summary>
        /// Get viewmodel for dataflow streams metadata result.
        /// </summary>
        [HealthCheckModuleMethod]
        public IEnumerable<DataflowStreamMetadata<TAccessRole>> GetDataflowStreamsMetadata(HealthCheckModuleContext context)
        {
            if (!context.HasAccess(AccessOption.FetchStream))
                return Enumerable.Empty<DataflowStreamMetadata<TAccessRole>>();

            return Options.DataflowService.GetStreamMetadata()
                .Where(x => context.HasRoleAccess(x.RolesWithAccess, defaultValue: true));
        }

        /// <summary>
        /// Get viewmodel for dataflow unified search metadatas.
        /// </summary>
        [HealthCheckModuleMethod]
        public IEnumerable<DataflowUnifiedSearchMetadata<TAccessRole>> GetDataflowUnifiedSearchMetadata(HealthCheckModuleContext context)
        {
            if (!context.HasAccess(AccessOption.UnifiedSearch))
                return Enumerable.Empty<DataflowUnifiedSearchMetadata<TAccessRole>>();

            return Options.DataflowService.GetUnifiedSearchesMetadata()
                .Where(x => context.HasRoleAccess(x.RolesWithAccess, defaultValue: true));
        }

        /// <summary>
        /// Get viewmodel for dataflow entries result.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCDataflowUnifiedSearchResult> UnifiedSearch(HealthCheckModuleContext context, HCDataFlowUnifiedSearchRequest model)
        {
            if (Options.DataflowService == null || !context.HasAccess(AccessOption.UnifiedSearch))
                return new HCDataflowUnifiedSearchResult();

            var metadatas = GetDataflowUnifiedSearchMetadata(context);
            if (!metadatas.Any())
                return new HCDataflowUnifiedSearchResult();

            var search = metadatas.FirstOrDefault(x => x.Id == model.SearchId);
            if (search != null)
            {
                context.AddAuditEvent(action: "Dataflow search fetched", subject: search?.Name)
                    .AddDetail("Query", model.Query);
            }

            return await Options.DataflowService.UnifiedSearchAsync(model.SearchId, model.Query, model.PageIndex, model.PageSize);
        }
        #endregion
    }
}
