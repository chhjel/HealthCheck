using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Metrics.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Metrics
{
    /// <summary>
    /// Module for viewing metrics.
    /// </summary>
    public class HCMetricsModule : HealthCheckModuleBase<HCMetricsModule.AccessOption>
    {
        private HCMetricsModuleOptions Options { get; }

        /// <summary>
        /// Module for viewing metrics.
        /// </summary>
        public HCMetricsModule(HCMetricsModuleOptions options)
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
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCMetricsModuleConfig();
        
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
        [HealthCheckModuleMethod]
        public async Task<GetMetricsViewModel> GetMetrics()
        {
            var data = await (Options.Storage?.GetCompiledMetricsDataAsync() ?? Task.FromResult(new CompiledMetricsData()));
            return new GetMetricsViewModel
            {
                GlobalCounters = data?.GlobalCounters ?? new Dictionary<string, CompiledMetricsCounterData>(),
                GlobalValues = data?.GlobalValues ?? new Dictionary<string, CompiledMetricsValueData>()
            };
        }
        #endregion
    }
}
