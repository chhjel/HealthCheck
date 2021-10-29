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
        : HCSingleBufferedListBlobStorageBase<HCEpiserverBlobSiteEventStorage.HCSiteEventBlobData, SiteEvent>, ISiteEventStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("808214C8-2881-49C9-9C6E-82E64257AF75");

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
        /// <para>Defaults to storing the last 1000 events.</para>
        /// </summary>
        public HCEpiserverBlobSiteEventStorage(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            SupportsMaxItemAge = true;
            _blobHelper = new HCEpiserverBlobHelper<HCSiteEventBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
            MaxItemCount = 1000;
            MaxItemAge = TimeSpan.FromDays(30);
        }

        /// <inheritdoc />
        public virtual Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
        {
            var data = GetBlobData();
            var items = data.Items
                .Where(x => x.Timestamp.ToUniversalTime() >= from && x.Timestamp.ToUniversalTime() <= to)
                .ToList();
            return Task.FromResult(items);
        }

        /// <inheritdoc />
        public virtual Task StoreEvent(SiteEvent siteEvent)
        {
            InsertItemBuffered(siteEvent);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task UpdateEvent(SiteEvent siteEvent)
        {
            UpdateItemBuffered(siteEvent.Id, x => x.SetValuesFrom(siteEvent));
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override object GetItemId(SiteEvent item) => item.Id;

        /// <inheritdoc />
        protected override DateTimeOffset GetItemTimestamp(SiteEvent item) => item.Timestamp;

        /// <inheritdoc />
        public virtual Task<SiteEvent> GetLastMergableEventOfType(string eventTypeId)
        {
            var data = GetBlobData();
            var match = data?.Items?.FirstOrDefault(x => x.EventTypeId == eventTypeId && x.AllowMerge);
            return Task.FromResult(match);
        }

        /// <inheritdoc />
        public virtual Task<SiteEvent> GetLastEventOfType(string eventTypeId)
        {
            var data = GetBlobData();
            var match = data?.Items?.FirstOrDefault(x => x.EventTypeId == eventTypeId);
            return Task.FromResult(match);
        }

        /// <inheritdoc />
        public virtual bool SupportsBlobs() => false;

        /// <inheritdoc />
        public virtual Task<string> GetBlob(Guid id) => Task.FromResult<string>(null);

        /// <inheritdoc />
        protected override HCSiteEventBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCSiteEventBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCSiteEventBlobData : IBufferedBlobListStorageData
        {
            /// <summary>
            /// All stored site events.
            /// </summary>
            public List<SiteEvent> Items { get; set; } = new List<SiteEvent>();
        }
    }
}
