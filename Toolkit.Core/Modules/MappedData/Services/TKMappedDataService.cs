using QoDL.Toolkit.Core.Modules.MappedData.Abstractions;
using QoDL.Toolkit.Core.Modules.MappedData.Models;
using QoDL.Toolkit.Core.Modules.MappedData.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.MappedData.Services;

/// <summary>
/// Handles things needed for the module to function.
/// </summary>
public class TKMappedDataService : ITKMappedDataService
{
    /// <summary>
    /// For use during debuging or hotreloading.
    /// </summary>
    public static bool DisableCache { get; set; }

    private static readonly Dictionary<string, TKMappedDataDefinitions> _definitionCache = new();

    /// <summary>
    /// Get all definitions in the given assemblies. Caches based on given assemblies.
    /// </summary>
    public TKMappedDataDefinitions GetDefinitions(IEnumerable<Assembly> assemblies, TKMappedDefinitionDiscoveryOptions options)
    {
        if (assemblies?.Any() != true) return new();

        var cacheKey = string.Join("|", assemblies.Select(x => x.FullName));
        lock (_definitionCache)
        {
            if (!_definitionCache.ContainsKey(cacheKey))
            {
                options ??= new TKMappedDefinitionDiscoveryOptions();
                var defs = TKMappedDataDefinitionBuilder.CreateDefinitions(assemblies, options);
                if (DisableCache) return defs;
                _definitionCache[cacheKey] = defs;
            }
            return _definitionCache[cacheKey];
        }
    }
}
