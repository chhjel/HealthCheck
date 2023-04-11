using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Helpers;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation
{
    /// <summary>
    /// Module for finding content with given permutations.
    /// </summary>
    public class TKContentPermutationModule : ToolkitModuleBase<TKContentPermutationModule.AccessOption>
    {
        private TKContentPermutationModuleOptions Options { get; }
        private static readonly ConcurrentDictionary<string, int> _lastPermutationCounts = new();

        /// <summary>
        /// Module for finding content with given permutations.
        /// </summary>
        public TKContentPermutationModule(TKContentPermutationModuleOptions options)
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
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKContentPermutationModuleConfig();
        
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
        [ToolkitModuleMethod]
        public Task<TKGetPermutationTypesViewModel> GetPermutationTypes(/*ToolkitModuleContext context*/)
        {
            var types = TKContentPermutationHelper.GetPermutationTypesCached(Options.AssembliesContainingPermutationTypes);
            var typeModels = types.Select(x => CreateViewModel(x)).ToList();
            var model = new TKGetPermutationTypesViewModel()
            {
                Types = typeModels
            };
            return Task.FromResult(model);
        }

        /// <summary>
        /// Find content from a given permutation.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<TKGetPermutatedContentViewModel> GetPermutatedContent(/*ToolkitModuleContext context, */ TKGetPermutatedContentRequest model)
        {
            var type = TKContentPermutationHelper.GetPermutationTypesCached(Options.AssembliesContainingPermutationTypes)
                .FirstOrDefault(x => x.Id == model.PermutationTypeId);
            var permutation = type?.Permutations?.FirstOrDefault(x => x.Id == model.PermutationChoiceId);
            if (permutation == null)
            {
                return new TKGetPermutatedContentViewModel { Content = new List<TKPermutatedContentItemViewModel>() };
            }

            var options = new TKGetContentPermutationContentOptions
            {
                Type = type.Type,
                PermutationObj = permutation.Choice,
                MaxCount = model.MaxCount
            };

            var contentResult = await Options.Service.GetContentForAsync(type, options);
            var content = contentResult.Content;
            var countKey = $"{type.Id}_{permutation.Id}";
            _lastPermutationCounts[countKey] = content.Count;

            return new TKGetPermutatedContentViewModel
            {
                Content = content
            };
        }
        #endregion

        #region Helpers
        private TKContentPermutationTypeViewModel CreateViewModel(TKContentPermutationType type)
        {
            var permutations = type.Permutations.Select(x => CreateViewModel(type, x)).ToList();
            return new TKContentPermutationTypeViewModel
            {
                Id = type.Id,
                Name = type.Name,
                Description = type.Description,
                MaxAllowedContentCount = type.MaxAllowedContentCount,
                DefaultContentCount = type.DefaultContentCount,
                Permutations = permutations,
                PropertyConfigs = type.PropertyConfigs
            };
        }

        private TKContentPermutationChoiceViewModel CreateViewModel(TKContentPermutationType type, TKContentPermutationChoice choice)
        {
            int? lastRetrievedCount = null;
            var countKey = $"{type.Id}_{choice.Id}";
            if (_lastPermutationCounts.TryGetValue(countKey, out var counter))
            {
                lastRetrievedCount = counter;
            }

            return new TKContentPermutationChoiceViewModel
            {
                Id = choice.Id,
                Choice = choice.Choice,
                LastRetrievedCount = lastRetrievedCount
            };
        }
        #endregion
    }
}
