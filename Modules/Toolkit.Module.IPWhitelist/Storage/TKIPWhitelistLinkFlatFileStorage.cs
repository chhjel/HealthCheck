﻿using Newtonsoft.Json;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Storage;

/// <summary></summary>
public class TKIPWhitelistLinkFlatFileStorage : ITKIPWhitelistLinkStorage
{
    private TKSimpleDataStoreWithId<TKIPWhitelistLink, Guid> Store { get; }

    /// <summary>
    /// Create a new <see cref="TKIPWhitelistLinkFlatFileStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    public TKIPWhitelistLinkFlatFileStorage(string filepath)
    {
        Store = new TKSimpleDataStoreWithId<TKIPWhitelistLink, Guid>(
            filepath,
            serializer: new Func<TKIPWhitelistLink, string>(JsonConvert.SerializeObject),
            deserializer: new Func<string, TKIPWhitelistLink>(JsonConvert.DeserializeObject<TKIPWhitelistLink>),
            idSelector: (x) => x.Id,
            idSetter: (x, id) => x.Id = id,
            nextIdFactory: (all, x) => Guid.NewGuid()
        );
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistLink> StoreRuleLinkAsync(TKIPWhitelistLink link)
    {
        var updatedLink = Store.InsertOrUpdateItem(link);
        return Task.FromResult(updatedLink);
    }

    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistLink>> GetRuleLinksAsync(Guid ruleId)
        => Task.FromResult(Store.GetEnumerable().Where(x => x.RuleId == ruleId));

    /// <inheritdoc />
    public Task DeleteRuleLinkAsync(Guid id)
    {
        Store.DeleteItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteRuleLinksAsync(Guid ruleId)
    {
        Store.DeleteWhere(x => x.RuleId == ruleId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistLink> GetRuleLinkFromSecretAsync(Guid ruleId, string secret)
        => Task.FromResult(Store.GetEnumerable().FirstOrDefault(x => x.RuleId == ruleId && x.Secret == secret));
}
