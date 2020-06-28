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
        public Task<List<AuditEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
             => Task.FromResult(Items.Where(x => 
                 x.Timestamp.ToUniversalTime() >= from.ToUniversalTime()
                 && x.Timestamp.ToUniversalTime() <= to.ToUniversalTime()
             ).ToList());

        /// <summary>
        /// Not supported in memory implementation. Returns null;
        /// </summary>
        public async Task<string> GetBlob(Guid id) => await Task.FromResult<string>(null);
    }
}
