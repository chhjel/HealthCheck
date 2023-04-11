using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.Metrics.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Metrics
{
    /// <summary>
    /// Module for viewing metrics.
    /// </summary>
    public class TKMetricsModule : ToolkitModuleBase<TKMetricsModule.AccessOption>
    {
        private TKMetricsModuleOptions Options { get; }

        /// <summary>
        /// Module for viewing metrics.
        /// </summary>
        public TKMetricsModule(TKMetricsModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.Storage == null) issues.Add("Options.Storage must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKMetricsModuleConfig();
        
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
        /// Get metrics.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<GetMetricsViewModel> GetMetrics()
        {
            var data = await (Options.Storage?.GetCompiledMetricsDataAsync() ?? Task.FromResult(new CompiledMetricsData()));
            return new GetMetricsViewModel
            {
                GlobalCounters = data?.GlobalCounters ?? new Dictionary<string, CompiledMetricsCounterData>(),
                GlobalValues = data?.GlobalValues ?? new Dictionary<string, CompiledMetricsValueData>(),
                GlobalNotes = data?.GlobalNotes ?? new Dictionary<string, CompiledMetricsNoteData>()
            };
        }
        #endregion
    }
}
