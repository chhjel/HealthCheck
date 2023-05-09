using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.IPWhitelist;

/// <summary>
/// Stores data in blob storage.
/// </summary>
public class TKEpiserverBlobIPWhitelistLinkStorage
    : TKSingleBufferedDictionaryBlobStorageBase<TKEpiserverBlobIPWhitelistLinkStorage.TKEpiserverBlobIPWhitelistLinkBlobData, TKIPWhitelistLink, Guid>, ITKIPWhitelistLinkStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("8888c69d-88d5-444a-8283-e33853a60e37");

    /// <summary>
    /// Defaults to the default provider if null.
    /// </summary>
    public string ProviderName { get; set; }

    /// <summary>
    /// Defaults to a hardcoded guid if null
    /// </summary>
    public Guid? ContainerId { get; set; }

    /// <summary>
    /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
    /// </summary>
    protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

    /// <inheritdoc />
    protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

    private readonly TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistLinkBlobData> _blobHelper;

    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public TKEpiserverBlobIPWhitelistLinkStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistLinkBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <inheritdoc />
    protected override TKEpiserverBlobIPWhitelistLinkBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEpiserverBlobIPWhitelistLinkBlobData data) => _blobHelper.StoreBlobData(data);

    #region ITKIPWhitelistLinkStorage Implementation
    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistLink>> GetRuleLinksAsync(Guid ruleId)
        => Task.FromResult(GetItems().Where(x => x.RuleId == ruleId));

    /// <inheritdoc />
    public Task DeleteRuleLinkAsync(Guid id)
    {
        RemoveItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteRuleLinksAsync(Guid ruleId)
    {
        RemoveAllItems(x => x.RuleId == ruleId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistLink> StoreRuleLinkAsync(TKIPWhitelistLink link)
    {
        if (link.Id == Guid.Empty)
        {
            link.Id = Guid.NewGuid();
        }
        InsertItemBuffered(link, link.Id);
        return Task.FromResult(link);
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistLink> GetRuleLinkFromSecretAsync(Guid ruleId, string secret)
        => Task.FromResult(GetItems().FirstOrDefault(x => x.RuleId == ruleId && x.Secret == secret));
    #endregion

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEpiserverBlobIPWhitelistLinkBlobData : IBufferedBlobDictionaryStorageData
    {
        /// <inheritdoc />
        public Dictionary<Guid, TKIPWhitelistLink> Items { get; set; } = new();
    }
}
