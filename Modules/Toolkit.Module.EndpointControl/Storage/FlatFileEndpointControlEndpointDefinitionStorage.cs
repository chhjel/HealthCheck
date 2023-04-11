using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.EndpointControl.Storage;

/// <summary>
/// Stores definition data on disk.
/// </summary>
public class FlatFileEndpointControlEndpointDefinitionStorage : IEndpointControlEndpointDefinitionStorage
{
    private TKSimpleDataStoreWithId<EndpointControlEndpointDefinition, string> Store { get; }
    private List<EndpointControlEndpointDefinition> _memoryCache = null;

    /// <summary>
    /// Create a new <see cref="FlatFileEndpointControlEndpointDefinitionStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public FlatFileEndpointControlEndpointDefinitionStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<EndpointControlEndpointDefinition, string>(
            filepath,
            serializer: new Func<EndpointControlEndpointDefinition, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, EndpointControlEndpointDefinition>((row) => JsonConvert.DeserializeObject<EndpointControlEndpointDefinition>(row)),
            idSelector: (x) => x.EndpointId,
            idSetter: (x, id) => x.EndpointId = id,
            nextIdFactory: (all, x) => x.EndpointId
        );
    }

    /// <summary>
    /// Clear all definitions.
    /// </summary>
    public async Task ClearAllDefinitions()
        => await Store.ClearDataAsync();

    /// <summary>
    /// Delete a single definition.
    /// </summary>
    public async Task DeleteDefinition(string endpointId)
    {
        Store.DeleteItem(endpointId);
        _memoryCache.RemoveAll(x => x.EndpointId == endpointId);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a definition for the given endpoint has been stored.
    /// </summary>
    public bool HasDefinitionFor(string endpointId)
    {
        EnsureMemoryCache();
        return _memoryCache.Any(x => x.EndpointId == endpointId);
    }

    /// <summary>
    /// Store a new definition.
    /// </summary>
    public void StoreDefinition(EndpointControlEndpointDefinition definition)
    {
        EnsureMemoryCache();
        if (!_memoryCache.Any(x => x.EndpointId == definition.EndpointId))
        {
            _memoryCache.Add(definition);
        }

        Store.InsertItem(definition);
    }

    /// <summary>
    /// Get all stored definitions.
    /// </summary>
    public IEnumerable<EndpointControlEndpointDefinition> GetDefinitions()
    {
        EnsureMemoryCache();
        return _memoryCache;
    }

    private void EnsureMemoryCache()
    {
        _memoryCache ??= Store.GetEnumerable().ToList();
    }
}
