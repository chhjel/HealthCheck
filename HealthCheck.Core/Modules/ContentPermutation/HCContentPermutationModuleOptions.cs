using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.ContentPermutation
{
    /// <summary>
    /// Options for <see cref="HCContentPermutationModule"/>.
    /// </summary>
    public class HCContentPermutationModuleOptions
    {
        /// <summary>
        /// The assemblies that contains classes decorated with <see cref="HCContentPermutationTypeAttribute"/>.
        /// </summary>
        public IEnumerable<Assembly> AssembliesContainingPermutationTypes { get; set; }

        /// <summary>
        /// Service that handles finding content for given permutations.
        /// </summary>
        public IHCContentPermutationContentDiscoveryService Service { get; set; }
    }
}
