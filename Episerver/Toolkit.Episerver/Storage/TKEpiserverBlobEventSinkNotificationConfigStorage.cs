using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores <see cref="EventSinkNotificationConfig"/>s.
/// </summary>
public class TKEpiserverBlobEventSinkNotificationConfigStorage
    : TKSingleBufferedDictionaryBlobStorageBase<TKEpiserverBlobEventSinkNotificationConfigStorage.TKEventSinkNotificationConfigBlobData, EventSinkNotificationConfig, Guid>, IEventSinkNotificationConfigStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("881e458c-310e-4f6a-a0e2-b12305cba8a7");

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

    private readonly TKEpiserverBlobHelper<TKEventSinkNotificationConfigBlobData> _blobHelper;

    /// <summary>
    /// Stores configs.
    /// <para>Defaults to storing the last 1000 events.</para>
    /// </summary>
    public TKEpiserverBlobEventSinkNotificationConfigStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        SupportsMaxItemAge = false;
        _blobHelper = new TKEpiserverBlobHelper<TKEventSinkNotificationConfigBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
    }

    #region IEventSinkNotificationConfigStorage Implementation
    /// <inheritdoc />
    public IEnumerable<EventSinkNotificationConfig> GetConfigs() => GetItems();

    /// <inheritdoc />
    public EventSinkNotificationConfig SaveConfig(EventSinkNotificationConfig config)
    {
        if (config.Id == Guid.Empty)
        {
            config.Id = Guid.NewGuid();
        }
        InsertItemBuffered(config, config.Id);
        return config;
    }

    /// <inheritdoc />
    public void DeleteConfig(Guid configId) => RemoveItem(configId);
    #endregion

    /// <inheritdoc />
    protected override TKEventSinkNotificationConfigBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKEventSinkNotificationConfigBlobData data) => _blobHelper.StoreBlobData(data);

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKEventSinkNotificationConfigBlobData : IBufferedBlobDictionaryStorageData
    {
        /// <summary>
        /// All stored configs.
        /// </summary>
        public Dictionary<Guid, EventSinkNotificationConfig> Items { get; set; } = new Dictionary<Guid, EventSinkNotificationConfig>();
    }
}
