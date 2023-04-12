using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;

/// <summary>
/// Storage for larger stringified audit blob objects.
/// </summary>
public interface IAuditBlobStorage
{
    /// <summary>
    /// Store an audit blob object and returns a generated id for it.
    /// </summary>
    Task<Guid> StoreBlob(string data);

    /// <summary>
    /// Get an audit blob object with the given id.
    /// </summary>
    Task<string> GetBlob(Guid id);

    /// <summary>
    /// Check if a given blob by id exists.
    /// </summary>
    Task<bool> HasBlob(Guid id);
}
