using Newtonsoft.Json;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.EndpointControl.Storage;

/// <summary>
/// Stores rule data on disk.
/// </summary>
public class FlatFileEndpointControlRuleStorage : IEndpointControlRuleStorage
{
    private TKSimpleDataStoreWithId<EndpointControlRule, Guid> Store { get; }
    private List<EndpointControlRule> _memoryCache = null;

    /// <summary>
    /// Create a new <see cref="FlatFileEndpointControlRuleStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public FlatFileEndpointControlRuleStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<EndpointControlRule, Guid>(
            filepath,
            serializer: new Func<EndpointControlRule, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, EndpointControlRule>((row) => JsonConvert.DeserializeObject<EndpointControlRule>(row)),
            idSelector: (x) => x.Id,
            idSetter: (x, id) => x.Id = id,
            nextIdFactory: (all, x) => Guid.NewGuid()
        );
    }

    /// <summary>
    /// Delete rule for the given rule id.
    /// </summary>
    public void DeleteRule(Guid ruleId)
    {
        Store.DeleteItem(ruleId);
        _memoryCache.RemoveAll(x => x.Id == ruleId);
    }

    /// <summary>
    /// Clear all rules.
    /// </summary>
    public async Task DeleteRules()
    {
        _memoryCache.Clear();
        await Store.ClearDataAsync();
    }

    /// <summary>
    /// Get a single rule by id.
    /// </summary>
    public EndpointControlRule GetRule(Guid id)
    {
        EnsureMemoryCache();
        return _memoryCache.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Get all rules.
    /// </summary>
    public IEnumerable<EndpointControlRule> GetRules()
    {
        EnsureMemoryCache();
        return _memoryCache;
    }

    /// <summary>
    /// Insert the given rule.
    /// </summary>
    public EndpointControlRule InsertRule(EndpointControlRule rule)
    {
        EnsureMemoryCache();
        if (!_memoryCache.Any(x => x.Id == rule.Id))
        {
            _memoryCache.Add(rule);
        }

        return Store.InsertItem(rule);
    }

    /// <summary>
    /// Updates the given rule.
    /// </summary>
    public EndpointControlRule UpdateRule(EndpointControlRule rule)
    {
        EnsureMemoryCache();
        _memoryCache.RemoveAll(x => x.Id == rule.Id);
        _memoryCache.Add(rule);

        return Store.InsertOrUpdateItem(rule);
    }

    private void EnsureMemoryCache()
    {
        _memoryCache ??= Store.GetEnumerable().ToList();
    }
}
