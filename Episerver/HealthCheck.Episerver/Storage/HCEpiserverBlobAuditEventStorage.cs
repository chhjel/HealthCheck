using EPiServer.Framework.Blobs;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Episerver.Utils;
using HealthCheck.Utility.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores audit events.
    /// </summary>
    public class HCEpiserverBlobAuditEventStorage
        : HCSingleBufferedListBlobStorageBase<HCEpiserverBlobAuditEventStorage.HCAuditEventsBlobData, AuditEvent>, IAuditEventStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("85814e08-cf34-4e69-97c2-63d3833f7967");

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

        private readonly EpiserverBlobHelper<HCAuditEventsBlobData> _blobHelper;

        /// <summary>
        /// Stores audit events.
        /// </summary>
        public HCEpiserverBlobAuditEventStorage(IBlobFactory blobFactory, IMemoryCache cache)
            : base(cache)
        {
            _blobHelper = new EpiserverBlobHelper<HCAuditEventsBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <inheritdoc />
        public virtual Task<List<AuditEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
        {
            var data = GetBlobData();
            var items = data.Items
                .Where(x => x.Timestamp.ToUniversalTime() >= from && x.Timestamp.ToUniversalTime() <= to)
                .ToList();
            return Task.FromResult(items);
        }

        /// <inheritdoc />
        public virtual Task StoreEvent(AuditEvent auditEvent)
        {
            InsertItemBuffered(auditEvent);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual bool SupportsBlobs() => false;

        /// <inheritdoc />
        public virtual Task<string> GetBlob(Guid id) => Task.FromResult<string>(null);

        /// <inheritdoc />
        protected override HCAuditEventsBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCAuditEventsBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCAuditEventsBlobData : IBufferedBlobListStorageData
        {
            /// <summary>
            /// All stored audit events.
            /// </summary>
            public List<AuditEvent> Items { get; set; } = new List<AuditEvent>();
        }
    }
}
