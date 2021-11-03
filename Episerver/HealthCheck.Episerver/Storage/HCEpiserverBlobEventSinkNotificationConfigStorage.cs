using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Models;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using System;
using System.Collections.Generic;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores <see cref="EventSinkNotificationConfig"/>s.
    /// </summary>
    public class HCEpiserverBlobEventSinkNotificationConfigStorage
        : HCSingleBufferedDictionaryBlobStorageBase<HCEpiserverBlobEventSinkNotificationConfigStorage.HCEventSinkNotificationConfigBlobData, EventSinkNotificationConfig, Guid>, IEventSinkNotificationConfigStorage
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
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly HCEpiserverBlobHelper<HCEventSinkNotificationConfigBlobData> _blobHelper;

        /// <summary>
        /// Stores configs.
        /// <para>Defaults to storing the last 1000 events.</para>
        /// </summary>
        public HCEpiserverBlobEventSinkNotificationConfigStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            SupportsMaxItemAge = false;
            _blobHelper = new HCEpiserverBlobHelper<HCEventSinkNotificationConfigBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
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
        protected override HCEventSinkNotificationConfigBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCEventSinkNotificationConfigBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCEventSinkNotificationConfigBlobData : IBufferedBlobDictionaryStorageData
        {
            /// <summary>
            /// All stored configs.
            /// </summary>
            public Dictionary<Guid, EventSinkNotificationConfig> Items { get; set; } = new Dictionary<Guid, EventSinkNotificationConfig>();
        }
    }
}
