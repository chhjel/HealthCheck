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
        /// <summary>
        /// For use during debuging or hotreloading.
        /// </summary>
        public static bool DisableCache { get; set; }

        private static readonly Dictionary<string, HCMappedDataDefinitions> _definitionCache = new();

        /// <summary>
        /// Get all definitions in the given assemblies. Caches based on given assemblies.
        /// </summary>
        public HCMappedDataDefinitions GetDefinitions(IEnumerable<Assembly> assemblies, HCMappedDefinitionDiscoveryOptions options)
        {
            if (assemblies?.Any() != true) return new();
            
            var cacheKey = string.Join("|", assemblies.Select(x => x.FullName));
            lock (_definitionCache)
            {
                if (!_definitionCache.ContainsKey(cacheKey))
                {
                    options ??= new HCMappedDefinitionDiscoveryOptions();
                    var defs = HCMappedDataDefinitionBuilder.CreateDefinitions(assemblies, options);
                    if (DisableCache) return defs;
                    _definitionCache[cacheKey] = defs;
                }
                return _definitionCache[cacheKey];
            }
        }
    }
}
