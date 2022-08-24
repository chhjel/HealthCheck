using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.ContentPermutation.Helpers;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ContentPermutation
{
    /// <summary>
    /// Module for finding content with given permutations.
    /// </summary>
    public class HCContentPermutationModule : HealthCheckModuleBase<HCContentPermutationModule.AccessOption>
    {
        private HCContentPermutationModuleOptions Options { get; }

        /// <summary>
        /// Module for finding content with given permutations.
        /// </summary>
        public HCContentPermutationModule(HCContentPermutationModuleOptions options)
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
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCContentPermutationModuleConfig();
        
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
        /// Get types.
        /// </summary>
        [HealthCheckModuleMethod]
        public Task<HCGetPermutationTypesViewModel> GetPermutationTypes(HealthCheckModuleContext context)
        {
            var types = HCContentPermutationHelper.GetPermutationTypesCached(Options.AssembliesContainingPermutationTypes);
            var model = new HCGetPermutationTypesViewModel()
            {
                Types = types
            };
            return Task.FromResult(model);
        }

        /// <summary>
        /// Find content from a given permutation.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<HCGetPermutatedContentViewModel> GetPermutatedContent(HealthCheckModuleContext context, HCGetPermutatedContentRequest model)
        {
            var type = HCContentPermutationHelper.GetPermutationTypesCached(Options.AssembliesContainingPermutationTypes)
                .FirstOrDefault(x => x.Id == model.PermutationTypeId);
            var permutation = type?.Permutations?.FirstOrDefault(x => x.Id == model.PermutationChoiceId);
            if (permutation == null)
            {
                return new HCGetPermutatedContentViewModel { Content = new List<HCPermutatedContentItemViewModel>() };
            }

            var content = await Options.Service.GetContentForAsync(type.Type, permutation.Choice);
            return new HCGetPermutatedContentViewModel
            {
                Content = content
            };
        }
        #endregion
    }
}
