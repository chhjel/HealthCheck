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
            Nothing = 0,
        }

        #region Invokable methods
        /// <summary>
        /// Get viewmodel for dataflow entries result.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<IEnumerable<IDataflowEntry>> GetDataflowStreamEntries(HealthCheckModuleContext context, GetDataflowStreamEntriesFilter model)
        {
            if (Options.DataflowService == null)
                return Enumerable.Empty<IDataflowEntry>();

            var metadatas = GetDataflowStreamsMetadata(context);
            if (!metadatas.Any())
                return Enumerable.Empty<IDataflowEntry>();

            var stream = metadatas.FirstOrDefault(x => x.Id == model.StreamId);
            if (stream != null)
            {
                context.AddAuditEvent(action: "Dataflow stream fetched", subject: stream?.Name)
                    .AddDetail("Stream id", stream?.Id)
                    .AddDetail("Filter input", model.StreamFilter?.CreateAuditSummary());
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
            return Options.DataflowService.GetStreamMetadata()
                .Where(x => context.HasRoleAccess(x.RolesWithAccess, defaultValue: true));
        }
        #endregion
    }
}
