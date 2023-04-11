using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation;

/// <summary>
/// Options for <see cref="TKContentPermutationModule"/>.
/// </summary>
public class TKContentPermutationModuleOptions
{
    /// <summary>
    /// The assemblies that contains classes decorated with <see cref="TKContentPermutationTypeAttribute"/>.
    /// </summary>
    public IEnumerable<Assembly> AssembliesContainingPermutationTypes { get; set; }

    /// <summary>
    /// Service that handles finding content for given permutations.
    /// </summary>
    public ITKContentPermutationContentDiscoveryService Service { get; set; }
}
