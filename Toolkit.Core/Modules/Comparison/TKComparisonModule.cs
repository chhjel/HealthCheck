using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Comparison
{
    /// <summary>
    /// Module for finding content with given permutations.
    /// </summary>
    public class TKComparisonModule : ToolkitModuleBase<TKComparisonModule.AccessOption>
    {
        private TKComparisonModuleOptions Options { get; }

        /// <summary>
        /// Module for finding content with given permutations.
        /// </summary>
        public TKComparisonModule(TKComparisonModuleOptions options)
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
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKComparisonModuleConfig();
        
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
        [ToolkitModuleMethod]
        public Task<List<TKComparisonTypeDefinition>> GetComparisonTypeDefinitions(/*ToolkitModuleContext context*/)
        {
            var types = Options.Service.GetComparisonTypeDefinitions();
            return Task.FromResult(types);
        }

        /// <summary>
        /// Get diff types.
        /// </summary>
        [ToolkitModuleMethod]
        public Task<Dictionary<string, List<TKComparisonDifferDefinition>>> GetDifferDefinitionsByHandlerId()
        {
            var types = Options.Service.GetDifferDefinitionsByHandlerId();
            return Task.FromResult(types);
        }

        /// <summary>
        /// Get instances to select.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<List<TKComparisonInstanceSelection>> GetFilteredOptions(TKGetFilteredOptionsRequestModel model)
        {
            var filter = new TKComparisonTypeFilter
            {
                Input = model.Input
            };
            return await Options.Service.GetFilteredOptionsAsync(model.HandlerId, filter);
        }

        /// <summary>
        /// Compare two instances.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<TKComparisonMultiDifferOutput> ExecuteDiff(TKExecuteDiffRequestModel model)
            => await Options.Service.ExecuteDiffAsync(model.HandlerId, model.DifferIds ?? new string[0], model.LeftId, model.RightId);
        #endregion
    }
}
