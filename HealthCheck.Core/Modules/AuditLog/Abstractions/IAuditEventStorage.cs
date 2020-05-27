using HealthCheck.Core.Modules.AuditLog.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.AuditLog.Abstractions
{
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
        Task<List<AuditEvent>> GetEvents(DateTime from, DateTime to);
    }
}
