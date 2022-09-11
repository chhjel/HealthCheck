using HealthCheck.Core.Modules.MappedData.Abstractions;
using HealthCheck.Core.Modules.MappedData.Models;
using HealthCheck.Core.Modules.MappedData.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.MappedData.Services
{
    /// <summary>
    /// Handles things needed for the module to function.
    /// </summary>
    public class HCMappedDataService : IHCMappedDataService
    {
        private static readonly Dictionary<string, List<HCMappedClassesDefinition>> _definitionCache = new();

        /// <summary>
        /// Get all definitions in the given assemblies. Caches based on given assemblies.
        /// </summary>
        public List<HCMappedClassesDefinition> GetDefinitions(IEnumerable<Assembly> assemblies, HCMappedDefinitionDiscoveryOptions options)
        {
            if (assemblies?.Any() != true) return new List<HCMappedClassesDefinition>();
            
            var cacheKey = string.Join("|", assemblies.Select(x => x.FullName));
            lock (_definitionCache)
            {
                if (!_definitionCache.ContainsKey(cacheKey))
                {
                    options ??= new HCMappedDefinitionDiscoveryOptions();
                    var defs = HCMappedDataDefinitionBuilder.CreateDefinitions(assemblies, options);
                    var pairs = HCMappedDataDefinitionBuilder.CreateDefinitionPairs(defs.Values);
                    _definitionCache[cacheKey] = pairs;
                }
                return _definitionCache[cacheKey];
            }
        }
    }
}
