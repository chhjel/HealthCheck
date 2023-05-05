using Newtonsoft.Json;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Storage;

/// <summary></summary>
public class TKIPWhitelistIPStorageFlatFileStorage : ITKIPWhitelistIPStorage
{
    private TKSimpleDataStoreWithId<TKIPWhitelistIP, Guid> Store { get; }

    /// <summary>
    /// Create a new <see cref="TKIPWhitelistIPStorageFlatFileStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public TKIPWhitelistIPStorageFlatFileStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TKIPWhitelistIP, Guid>(
            filepath,
            serializer: new Func<TKIPWhitelistIP, string>(JsonConvert.SerializeObject),
            deserializer: new Func<string, TKIPWhitelistIP>(JsonConvert.DeserializeObject<TKIPWhitelistIP>),
            idSelector: (x) => x.Id,
            idSetter: (x, id) => x.Id = id,
            nextIdFactory: (all, x) => Guid.NewGuid()
        );
    }

    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistIP>> GetRuleIPsAsync(Guid ruleId)
        => Task.FromResult(Store.GetEnumerable().Where(x => x.RuleId == ruleId));

    /// <inheritdoc />
    public Task DeleteRuleIPAsync(Guid id)
    {
        Store.DeleteItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteRuleIPsAsync(Guid ruleId)
    {
        Store.DeleteWhere(x => x.RuleId == ruleId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistIP> StoreRuleIPAsync(TKIPWhitelistIP ip)
    {
        var updatedLink = Store.InsertOrUpdateItem(ip);
        return Task.FromResult(updatedLink);
    }

    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistIP>> GetAllIPsAsync()
        => Task.FromResult(Store.GetEnumerable());
}
