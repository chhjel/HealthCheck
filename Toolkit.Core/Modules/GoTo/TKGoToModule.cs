using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.GoTo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.GoTo
{
    /// <summary>
    /// Module for locating anything by its id or some value.
    /// </summary>
    public class TKGoToModule : ToolkitModuleBase<TKGoToModule.AccessOption>
    {
        private TKGoToModuleOptions Options { get; }

        /// <summary>
        /// Module for locating anything by its id or some value.
        /// </summary>
        public TKGoToModule(TKGoToModuleOptions options)
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
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKGoToModuleConfig();
        
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
        [ToolkitModuleMethod]
        public Task<List<TKGoToResolverDefinition>> GetResolversDefinitions(/*ToolkitModuleContext context*/)
        {
            var types = Options.Service.GetResolvers();
            return Task.FromResult(types);
        }

        /// <summary>
        /// Attempt to locate an instance.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<List<TKGoToResolvedDataWithResolverId>> Goto(TKGoToRequestModel model)
            => await Options.Service.TryLocateInstance(model.HandlerIds, model.Input);
        #endregion
    }
}
