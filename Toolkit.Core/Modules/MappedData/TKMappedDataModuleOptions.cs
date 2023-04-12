using QoDL.Toolkit.Core.Modules.MappedData.Abstractions;
using QoDL.Toolkit.Core.Modules.MappedData.Models;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.MappedData;

/// <summary>
/// Options for <see cref="TKMappedDataModule"/>.
/// </summary>
public class TKMappedDataModuleOptions
{
    /// <summary>
    /// Assemblies to build definitions from.
    /// </summary>
    public IEnumerable<Assembly> IncludedAssemblies { get; set; }

    /// <summary>
    /// Options for discovery of types to create definitions from.
    /// </summary>
    public TKMappedDefinitionDiscoveryOptions DiscoveryOptions { get; set; }

    /// <summary>
    /// Service that handles required module things.
    /// </summary>
    public ITKMappedDataService Service { get; set; }
}
