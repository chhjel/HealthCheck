using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Models;
using QoDL.Toolkit.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// Stores events to the given file.
/// </summary>
public class FlatFileAuditEventStorage : IAuditEventStorage
{
    private TKSimpleDataStore<AuditEvent> Store { get; set; }
    private readonly IAuditBlobStorage _blobStorage;

    /// <summary>
    /// Create a new <see cref="FlatFileAuditEventStorage"/> with the given file path.
    /// </summary>
    /// <param name="filepath">Filepath to where the data will be stored.</param>
    /// <param name="maxEventAge">Max age of entries before they can become deleted. Leave at null to disable cleanup.</param>
    /// <param name="delayFirstCleanup">Delay first cleanup by the lowest of 4 hours or max event age.</param>
    /// <param name="blobStorage">Optional implementation that stores larger blob data.</param>
    public FlatFileAuditEventStorage(string filepath,
        TimeSpan? maxEventAge = null,
        bool delayFirstCleanup = true,
        IAuditBlobStorage blobStorage = null)
    {
        Store = new TKSimpleDataStore<AuditEvent>(
            filepath,
            serializer: new Func<AuditEvent, string>((e) => JsonConvert.SerializeObject(e)),
            deserializer: new Func<string, AuditEvent>((row) => JsonConvert.DeserializeObject<AuditEvent>(row))
        );

        if (maxEventAge != null)
        {
            var minimumCleanupInterval = TimeSpan.FromHours(4);
            Store.RetentionOptions = new TKStorageRetentionOptions<AuditEvent>(
                (item) => item.Timestamp,
                maxAge: maxEventAge.Value,
                minimumCleanupInterval: (maxEventAge.Value < minimumCleanupInterval) ? maxEventAge.Value : minimumCleanupInterval,
                delayFirstCleanup: delayFirstCleanup
            );
        }

        this._blobStorage = blobStorage;
    }

    /// <summary>
    /// Store the given event. There is a 2 second buffer delay before the item is written.
    /// </summary>
    public async Task StoreEvent(AuditEvent auditEvent)
    {
        var contents = auditEvent?.GetBlobs();
        auditEvent?.BlobIds?.Clear();

        if (_blobStorage != null && contents?.Any() == true)
        {
            foreach(var kvp in contents)
            {
                var id = await _blobStorage.StoreBlob(kvp.Value);
                auditEvent.BlobIds.Add(new KeyValuePair<string, Guid>(kvp.Key, id));
            }
        }

        Store.InsertItem(auditEvent);
    }

    /// <summary>
    /// Get stored events within the given threshold.
    /// </summary>
    public Task<List<AuditEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
    {
        var items = Store.GetEnumerable()
            .Where(x => x.Timestamp.ToUniversalTime() >= from && x.Timestamp.ToUniversalTime() <= to)
            .ToList();

        return Task.FromResult(items);
    }

    /// <summary>
    /// Get the contents of an audit event blob.
    /// <para>For this to return data, an <see cref="IAuditBlobStorage"/> implementation must be provided in the constructor.</para>
    /// </summary>
    public async Task<string> GetBlob(Guid id)
    {
        if (_blobStorage == null)
        {
            return null;
        }

        var exists = await _blobStorage.HasBlob(id);
        if (!exists)
        {
            return null;
        }

        return await _blobStorage.GetBlob(id);
    }

    /// <summary>
    /// Returns true if an <see cref="IAuditBlobStorage"/> implementation was given in the constructor.
    /// </summary>
    public bool SupportsBlobs() => _blobStorage != null;
}
