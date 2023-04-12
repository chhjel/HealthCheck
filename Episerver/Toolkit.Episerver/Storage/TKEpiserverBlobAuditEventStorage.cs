using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Models;
using QoDL.Toolkit.Episerver.Utils;
using QoDL.Toolkit.Core.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores audit events.
/// </summary>
public class TKEpiserverBlobAuditEventStorage
    : TKSingleBufferedListBlobStorageBase<TKEpiserverBlobAuditEventStorage.TKAuditEventsBlobData, AuditEvent>, IAuditEventStorage
{
    /// <summary>
    /// Container id used if not overridden.
    /// </summary>
    protected virtual Guid DefaultContainerId => Guid.Parse("86814e08-cf34-4e69-97c2-63d3833f7967");

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

    private readonly TKEpiserverBlobHelper<TKAuditEventsBlobData> _blobHelper;

    /// <summary>
    /// Stores audit events.
    /// </summary>
    public TKEpiserverBlobAuditEventStorage(IBlobFactory blobFactory, ITKCache cache)
        : base(cache)
    {
        SupportsMaxItemAge = true;
        _blobHelper = new TKEpiserverBlobHelper<TKAuditEventsBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        MaxItemCount = 10000;
        MaxItemAge = TimeSpan.FromDays(30);
    }

    /// <inheritdoc />
    public virtual Task<List<AuditEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
    {
        var items = GetItems()
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
    protected override TKAuditEventsBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

    /// <inheritdoc />
    protected override void StoreBlobData(TKAuditEventsBlobData data) => _blobHelper.StoreBlobData(data);

    /// <inheritdoc />
    protected override DateTimeOffset GetItemTimestamp(AuditEvent item) => item.Timestamp;

    /// <summary>
    /// Model stored in blob storage.
    /// </summary>
    public class TKAuditEventsBlobData : IBufferedBlobListStorageData
    {
        /// <summary>
        /// All stored audit events.
        /// </summary>
        public List<AuditEvent> Items { get; set; } = new List<AuditEvent>();
    }
}
