using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.GoTo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.GoTo
{
    /// <summary>
    /// Module for locating anything by its id or some value.
    /// </summary>
    public class HCGoToModule : HealthCheckModuleBase<HCGoToModule.AccessOption>
    {
        private HCGoToModuleOptions Options { get; }

        /// <summary>
        /// Module for locating anything by its id or some value.
        /// </summary>
        public HCGoToModule(HCGoToModuleOptions options)
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
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCGoToModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0
        }

        #region Invokable methods
        /// <summary>
        /// Get resolver definitions.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<List<HCGoToResolverDefinition>> GetResolversDefinitions(/*HealthCheckModuleContext context*/)
        {
            var types = Options.Service.GetResolvers();
            return Task.FromResult(types);
        }

        /// <summary>
        /// Attempt to locate an instance.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<List<HCGoToResolvedDataWithResolverId>> Goto(HCGoToRequestModel model)
            => await Options.Service.TryLocateInstance(model.HandlerIds, model.Input);
        #endregion
    }
}
