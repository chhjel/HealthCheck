using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Comparison
{
    /// <summary>
    /// Module for finding content with given permutations.
    /// </summary>
    public class HCComparisonModule : HealthCheckModuleBase<HCComparisonModule.AccessOption>
    {
        private HCComparisonModuleOptions Options { get; }

        /// <summary>
        /// Module for finding content with given permutations.
        /// </summary>
        public HCComparisonModule(HCComparisonModuleOptions options)
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
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCComparisonModuleConfig();
        
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
        /// Get content types.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<List<HCComparisonTypeDefinition>> GetComparisonTypeDefinitions(/*HealthCheckModuleContext context*/)
        {
            var types = Options.Service.GetComparisonTypeDefinitions();
            return Task.FromResult(types);
        }

        /// <summary>
        /// Get diff types.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<Dictionary<string, List<HCComparisonDifferDefinition>>> GetDifferDefinitionsByHandlerId()
        {
            var types = Options.Service.GetDifferDefinitionsByHandlerId();
            return Task.FromResult(types);
        }

        /// <summary>
        /// Get content types.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<List<HCComparisonInstanceSelection>> GetFilteredOptions(HCGetFilteredOptionsRequestModel model)
        {
            var filter = new HCComparisonTypeFilter
            {
                Input = model.Input
            };
            return await Options.Service.GetFilteredOptionsAsync(model.HandlerId, filter);
        }

        /// <summary>
        /// Get content types.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCComparisonMultiDifferOutput> ExecuteDiff(HCExecuteDiffRequestModel model)
            => await Options.Service.ExecuteDiffAsync(model.HandlerId, model.DifferIds ?? new string[0], model.LeftId, model.RightId);
        #endregion
    }
}
