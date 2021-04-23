using EPiServer.Framework.Blobs;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Episerver.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores audit events.
    /// </summary>
    public class HCEpiserverBlobAuditEventStorage : HCEpiserverSingleBlobStorageBase<HCEpiserverBlobAuditEventStorage.HCAuditEventsBlobData>, IAuditEventStorage
    {
        /// <inheritdoc />
        protected override Guid DefaultContainerId => Guid.Parse("85814e08-cf34-4e69-97c2-63d3833f7967");

        /// <summary>
        /// Stores audit events.
        /// </summary>
        public HCEpiserverBlobAuditEventStorage(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {
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
            var data = GetBlobData();
            if (data != null)
            {
                data.Items.Add(auditEvent);
                SaveBlobData(data);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual bool SupportsBlobs() => false;

        /// <inheritdoc />
        public virtual Task<string> GetBlob(Guid id) => Task.FromResult<string>(null);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCAuditEventsBlobData
        {
            /// <summary>
            /// All stored audit events.
            /// </summary>
            public ConcurrentBag<AuditEvent> Items { get; set; } = new ConcurrentBag<AuditEvent>();
        }
    }
}
