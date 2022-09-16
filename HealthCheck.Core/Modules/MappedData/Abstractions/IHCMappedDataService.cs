using HealthCheck.Core.Modules.MappedData.Models;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.MappedData.Abstractions
{
    /// <summary>
    /// Handles things needed for the module to function.
    /// </summary>
    public interface IHCMappedDataService
    {
        /// <summary>
        /// Get all definitions in the given assemblies. Caches based on given assemblies.
        /// </summary>
        HCMappedDataDefinitions GetDefinitions(IEnumerable<Assembly> assemblies, HCMappedDefinitionDiscoveryOptions options);
    }
}