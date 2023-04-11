using QoDL.Toolkit.Core.Modules.AuditLog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;

/// <summary>
/// Storage for <see cref="AuditEvent"/> objects.
/// </summary>
public interface IAuditEventStorage
{
    /// <summary>
    /// Store a <see cref="AuditEvent"/> object.
    /// </summary>
    Task StoreEvent(AuditEvent auditEvent);

    /// <summary>
    /// Get all stored <see cref="AuditEvent"/>s objects with a <see cref="AuditEvent.Timestamp"/> within the given threshold.
    /// </summary>
    Task<List<AuditEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to);

    /// <summary>
    /// Get the contents of an audit event blob, or null if not found.
    /// <para>Optionally implemented.</para>
    /// </summary>
    Task<string> GetBlob(Guid id);

    /// <summary>
    /// Returns true if blobs are supported.
    /// </summary>
    public bool SupportsBlobs();
}
