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
public class TKEpiserverBlobIPWhitelistIPStorage
    : TKSingleBufferedListBlobStorageBase<TKEpiserverBlobIPWhitelistIPStorage.TKEpiserverBlobIPWhitelistIPBlobData, TKIPWhitelistIP>, ITKIPWhitelistIPStorage
{
    /// <inheritdoc />
    public bool SupportsCache { get; } = true;

    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("888862ab-4f43-4fcd-a681-54adec6dbab3");

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

    private readonly TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistIPBlobData> _blobHelper;

    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public TKEpiserverBlobIPWhitelistIPStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistIPBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <inheritdoc />
    protected override TKEpiserverBlobIPWhitelistIPBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEpiserverBlobIPWhitelistIPBlobData data) => _blobHelper.StoreBlobData(data);

    #region ITKIPWhitelistIPStorage Implementation
    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistIP>> GetAllIPsAsync()
        => Task.FromResult(GetItems());

    /// <inheritdoc />
    public Task<IEnumerable<TKIPWhitelistIP>> GetRuleIPsAsync(Guid ruleId)
        => Task.FromResult(GetItems().Where(x => x.RuleId == ruleId));

    /// <inheritdoc />
    public Task DeleteRuleIPAsync(Guid id)
    {
        RemoveMatching(x => x.Id == id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteRuleIPsAsync(Guid ruleId)
    {
        RemoveMatching(x => x.RuleId == ruleId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TKIPWhitelistIP> StoreRuleIPAsync(TKIPWhitelistIP ip)
    {
        if (ip.Id == Guid.Empty)
        {
            ip.Id = Guid.NewGuid();
        }
        InsertItemBuffered(ip, ip.Id);
        return Task.FromResult(ip);
    }
    #endregion

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEpiserverBlobIPWhitelistIPBlobData : IBufferedBlobListStorageData
    {
        /// <inheritdoc />
        public List<TKIPWhitelistIP> Items { get; set; } = new();
    }
}
