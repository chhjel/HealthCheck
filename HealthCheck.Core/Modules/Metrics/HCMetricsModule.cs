using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Metrics.Models;
using System;
using System.Collections.Generic;

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
            //if (Options.Service == null) issues.Add("Options.Service must be set.");
            //if (Options.ModelType == null) issues.Add("Options.ModelType must be set.");
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
        /// Get settings.
        /// </summary>
        [HealthCheckModuleMethod]
        public GetMetricsViewModel GetMetrics()
        {
            return new GetMetricsViewModel()
            {
            };
        }
        #endregion
    }
}
