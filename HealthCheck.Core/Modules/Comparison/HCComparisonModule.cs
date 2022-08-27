using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Collections.Generic;

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
            //if (Options.Service == null) issues.Add("Options.Service must be set.");
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
        ///// <summary>
        ///// Get types.
        ///// </summary>
        //[HealthCheckModuleMethod]
        //public Task<HCGetPermutationTypesViewModel> GetPermutationTypes(/*HealthCheckModuleContext context*/)
        //{
        //    var types = HCContentPermutationHelper.GetPermutationTypesCached(Options.AssembliesContainingPermutationTypes);
        //    var typeModels = types.Select(x => CreateViewModel(x)).ToList();
        //    var model = new HCGetPermutationTypesViewModel()
        //    {
        //        Types = typeModels
        //    };
        //    return Task.FromResult(model);
        //}

        ///// <summary>
        ///// Find content from a given permutation.
        ///// </summary>
        //[HealthCheckModuleMethod]
        //public async Task<HCGetPermutatedContentViewModel> GetPermutatedContent(/*HealthCheckModuleContext context, */ HCGetPermutatedContentRequest model)
        //{
        //    var type = HCContentPermutationHelper.GetPermutationTypesCached(Options.AssembliesContainingPermutationTypes)
        //        .FirstOrDefault(x => x.Id == model.PermutationTypeId);
        //    var permutation = type?.Permutations?.FirstOrDefault(x => x.Id == model.PermutationChoiceId);
        //    if (permutation == null)
        //    {
        //        return new HCGetPermutatedContentViewModel { Content = new List<HCPermutatedContentItemViewModel>() };
        //    }

        //    var options = new HCGetContentPermutationContentOptions
        //    {
        //        Type = type.Type,
        //        PermutationObj = permutation.Choice,
        //        MaxCount = model.MaxCount
        //    };

        //    var contentResult = await Options.Service.GetContentForAsync(type, options);
        //    var content = contentResult.Content;
        //    var countKey = $"{type.Id}_{permutation.Id}";
        //    _lastPermutationCounts[countKey] = content.Count;

        //    return new HCGetPermutatedContentViewModel
        //    {
        //        Content = content
        //    };
        //}
        #endregion

        #region Helpers
        //private HCContentPermutationTypeViewModel CreateViewModel(HCContentPermutationType type)
        //{
        //    var permutations = type.Permutations.Select(x => CreateViewModel(type, x)).ToList();
        //    return new HCContentPermutationTypeViewModel
        //    {
        //        Id = type.Id,
        //        Name = type.Name,
        //        Description = type.Description,
        //        MaxAllowedContentCount = type.MaxAllowedContentCount,
        //        DefaultContentCount = type.DefaultContentCount,
        //        Permutations = permutations,
        //        PropertyConfigs = type.PropertyConfigs
        //    };
        //}
        #endregion
    }
}
