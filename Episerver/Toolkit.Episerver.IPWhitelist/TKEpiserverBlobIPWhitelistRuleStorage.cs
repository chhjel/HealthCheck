using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.IPWhitelist;

/// <summary>
/// Stores data in blob storage.
/// </summary>
public class TKEpiserverBlobIPWhitelistRuleStorage
    : TKSingleBufferedDictionaryBlobStorageBase<TKEpiserverBlobIPWhitelistRuleStorage.TKEpiserverBlobIPWhitelistRuleBlobData, TKIPWhitelistRule, Guid>, ITKIPWhitelistRuleStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("88889df5-8229-4363-95a9-1354a4914426");

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

    private readonly TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistRuleBlobData> _blobHelper;

    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public TKEpiserverBlobIPWhitelistRuleStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistRuleBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <inheritdoc />
    protected override TKEpiserverBlobIPWhitelistRuleBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEpiserverBlobIPWhitelistRuleBlobData data) => _blobHelper.StoreBlobData(data);

    #region ITKIPWhitelistRuleStorage Implementation
    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistRule>> GetRulesAsync()
        => Task.FromResult(GetItems());

    /// <inheritdoc />
    public Task<TKIPWhitelistRule> GetRuleAsync(Guid id)
        => Task.FromResult(GetItems().FirstOrDefault(x => x.Id == id));

    /// <inheritdoc />
    public Task DeleteRuleAsync(Guid id)
    {
        RemoveItem(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistRule> StoreRuleAsync(TKIPWhitelistRule rule)
    {
        if (rule.Id == Guid.Empty)
        {
            rule.Id = Guid.NewGuid();
        }
        InsertItemBuffered(rule, rule.Id);
        return Task.FromResult(rule);
    }
    #endregion

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEpiserverBlobIPWhitelistRuleBlobData : IBufferedBlobDictionaryStorageData
    {
        /// <inheritdoc />
        public Dictionary<Guid, TKIPWhitelistRule> Items { get; set; }
    }
}
