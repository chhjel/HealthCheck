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
public class TKEpiserverBlobIPWhitelistConfigStorage
    : TKSingleBufferedListBlobStorageBase<TKEpiserverBlobIPWhitelistConfigStorage.TKEpiserverBlobIPWhitelistConfigBlobData, TKIPWhitelistConfig>, ITKIPWhitelistConfigStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("8888b32c-bf8b-433b-b77a-4ba60f51cf9c");

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

    private readonly TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistConfigBlobData> _blobHelper;
    private static readonly Guid _configGuid = Guid.Parse("2d6a5b49-429e-451f-aec1-5e21e0959bdb");

    /// <summary>
    /// Stores data in blob storage.
    /// </summary>
    public TKEpiserverBlobIPWhitelistConfigStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        _blobHelper = new TKEpiserverBlobHelper<TKEpiserverBlobIPWhitelistConfigBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    /// <inheritdoc />
    protected override TKEpiserverBlobIPWhitelistConfigBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEpiserverBlobIPWhitelistConfigBlobData data) => _blobHelper.StoreBlobData(data);

    #region ITKIPWhitelistConfigStorage Implementation
    /// <inheritdoc />
    public Task<TKIPWhitelistConfig> GetConfigAsync() => Task.FromResult(GetItems().FirstOrDefault() ?? new());

    /// <inheritdoc />
    public Task SaveConfigAsync(TKIPWhitelistConfig config)
    {
        InsertItemBuffered(config, _configGuid);
        return Task.CompletedTask;
    }
    #endregion

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEpiserverBlobIPWhitelistConfigBlobData : IBufferedBlobListStorageData
    {
        /// <inheritdoc />
        public List<TKIPWhitelistConfig> Items { get; set; } = new();
    }
}
