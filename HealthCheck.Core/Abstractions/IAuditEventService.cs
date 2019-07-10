using HealthCheck.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Storage for <see cref="AuditEvent"/> objects.
    /// </summary>
    public interface IAuditEventService
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
