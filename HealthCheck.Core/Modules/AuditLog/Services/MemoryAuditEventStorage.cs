using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.AuditLog.Services
{
    /// <summary>
    /// Stores in memory only.
    /// </summary>
    public class MemoryAuditEventStorage : IAuditEventStorage
    {
        private readonly ConcurrentBag<AuditEvent> Items = new ConcurrentBag<AuditEvent>();

        /// <summary>
        /// Store the given event in memory.
        /// </summary>
        public Task StoreEvent(AuditEvent auditEvent)
        {
            Items.Add(auditEvent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get some events from memory.
        /// </summary>
        public Task<List<AuditEvent>> GetEvents(DateTime from, DateTime to)
             => Task.FromResult(Items.Where(x => x.Timestamp >= from && x.Timestamp <= to).ToList());
    }
}
