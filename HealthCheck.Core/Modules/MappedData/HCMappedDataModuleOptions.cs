using HealthCheck.Core.Modules.MappedData.Abstractions;
using HealthCheck.Core.Modules.MappedData.Models;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.MappedData
{
    /// <summary>
    /// Options for <see cref="HCMappedDataModule"/>.
    /// </summary>
    public class HCMappedDataModuleOptions
    {
        /// <summary>
        /// Assemblies to build definitions from.
        /// </summary>
        public IEnumerable<Assembly> IncludedAssemblies { get; set; }

        /// <summary>
        /// Options for discovery of types to create definitions from.
        /// </summary>
        public HCMappedDefinitionDiscoveryOptions DiscoveryOptions { get; set; }

        /// <summary>
        /// Service that handles required module things.
        /// </summary>
        public IHCMappedDataService Service { get; set; }
    }
}
