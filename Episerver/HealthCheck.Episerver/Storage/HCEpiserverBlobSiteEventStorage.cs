using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores site events.
    /// <para>Defaults to storing the last 1000 events, and max 30 days old.</para>
    /// </summary>
    public class HCEpiserverBlobSiteEventStorage
        : HCSingleBufferedDictionaryBlobStorageBase<HCEpiserverBlobSiteEventStorage.HCSiteEventBlobData, SiteEvent, Guid>, ISiteEventStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("808216C8-2883-49C9-9C6E-82E64257AF75");

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

        private readonly HCEpiserverBlobHelper<HCSiteEventBlobData> _blobHelper;

        /// <summary>
        /// Stores site events.
        /// <para>Defaults to storing the last 1000 events / 30 days.</para>
        /// </summary>
        public HCEpiserverBlobSiteEventStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            _blobHelper = new HCEpiserverBlobHelper<HCSiteEventBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
            BlobUpdateBufferDuration = TimeSpan.FromSeconds(30);
            MaxItemCount = 1000;
            SupportsMaxItemAge = true;
            MaxItemAge = TimeSpan.FromDays(30);
        }

        #region ISiteEventStorage Implementation
        /// <inheritdoc />
        public Task<SiteEvent> GetEvent(Guid id)
        {
            var item = GetItems().FirstOrDefault(x => x.Id == id);
            return Task.FromResult(item);
        }

        /// <inheritdoc />
        public virtual Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
        {
            var items = GetItems()
                .Where(x => x.Timestamp.ToUniversalTime() >= from && x.Timestamp.ToUniversalTime() <= to)
                .ToList();
            return Task.FromResult(items);
        }

        /// <inheritdoc />
        public virtual Task StoreEvent(SiteEvent siteEvent)
        {
            InsertItemBuffered(siteEvent, siteEvent.Id, siteEvent.EventTypeId);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task UpdateEvent(SiteEvent siteEvent)
        {
            InsertItemBuffered(siteEvent, siteEvent.Id, siteEvent.EventTypeId, isUpdate: true);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId)
        {
            var match = GetItems()?.FirstOrDefault(x => x.EventTypeId == eventTypeId && x.AllowMerge);
            return Task.FromResult(match);
        }

        /// <inheritdoc />
        public virtual Task<SiteEvent> GetLastUnresolvedEventOfType(string eventTypeId)
        {
            var match = GetItems()
                    ?.Where(x => x.EventTypeId == eventTypeId && !x.Resolved)
                    ?.OrderByDescending(x => x.Timestamp)
                    ?.FirstOrDefault();
            return Task.FromResult(match);
        }

        /// <inheritdoc />
        public Task<IEnumerable<SiteEvent>> GetUnresolvedEventsOfType(string eventTypeId)
        {
            var items = GetItems()
                .Where(x => x.EventTypeId == eventTypeId && !x.Resolved)
                .AsEnumerable();
            return Task.FromResult(items);
        }

        /// <inheritdoc />
        public Task DeleteAllEvents()
        {
            BufferQueue.Clear();

            var data = GetBlobData();
            data.Items.Clear();
            SaveBlobData(data);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteEvent(Guid id)
        {
            RemoveItem(id);
            return Task.CompletedTask;
        }
        #endregion

        /// <inheritdoc />
        protected override DateTimeOffset GetItemTimestamp(SiteEvent item) => item.Timestamp;

        /// <inheritdoc />
        protected override HCSiteEventBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCSiteEventBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCSiteEventBlobData : IBufferedBlobDictionaryStorageData
        {
            /// <summary>
            /// All stored site events.
            /// </summary>
            public Dictionary<Guid, SiteEvent> Items { get; set; } = new Dictionary<Guid, SiteEvent>();
        }
    }
}
