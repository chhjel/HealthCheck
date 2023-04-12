using QoDL.Toolkit.Core.Modules.MappedData.Models;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.MappedData.Abstractions;

/// <summary>
/// Handles things needed for the module to function.
/// </summary>
public interface ITKMappedDataService
{
    /// <summary>
    /// Get all definitions in the given assemblies. Caches based on given assemblies.
    /// </summary>
    TKMappedDataDefinitions GetDefinitions(IEnumerable<Assembly> assemblies, TKMappedDefinitionDiscoveryOptions options);
}