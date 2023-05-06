using Newtonsoft.Json;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Storage;

/// <summary></summary>
public class TKIPWhitelistRuleFlatFileStorage : ITKIPWhitelistRuleStorage
{
    private TKSimpleDataStoreWithId<TKIPWhitelistRule, Guid> Store { get; }

    /// <summary>
    /// Create a new <see cref="TKIPWhitelistRuleFlatFileStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public TKIPWhitelistRuleFlatFileStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TKIPWhitelistRule, Guid>(
            filepath,
            serializer: new Func<TKIPWhitelistRule, string>(JsonConvert.SerializeObject),
            deserializer: new Func<string, TKIPWhitelistRule>((row) => JsonConvert.DeserializeObject<TKIPWhitelistRule>(row)),
            idSelector: (x) => x.Id,
            idSetter: (x, id) => x.Id = id,
            nextIdFactory: (all, x) => Guid.NewGuid()
        );
    }

    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistRule>> GetRulesAsync() => Task.FromResult(Store.GetEnumerable());

    /// <inheritdoc />
    public Task<TKIPWhitelistRule> GetRuleAsync(Guid id) => Task.FromResult(Store.GetItem(id));

    /// <inheritdoc />
    public Task DeleteRuleAsync(Guid id)
    {
        Store.DeleteItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistRule> StoreRuleAsync(TKIPWhitelistRule rule)
    {
        var updatedRule = Store.InsertOrUpdateItem(rule);
        return Task.FromResult(updatedRule);
    }
}
