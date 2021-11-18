using HealthCheck.Core.Modules.AuditLog.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.AuditLog.Services
{
    /// <summary>
    /// Stores in memory only.
    /// </summary>
    public class MemoryAuditBlobStorage : IAuditBlobStorage
    {
        private readonly Dictionary<Guid, string> _data = new();

        /// <summary>
        /// Store the given blob in memory.
        /// </summary>
        public async Task<Guid> StoreBlob(string data)
        {
            var id = Guid.NewGuid();
            _data[id] = data;
            return await Task.FromResult(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the given blob from memory.
        /// </summary>
        public async Task<string> GetBlob(Guid id)
            => (!await HasBlob(id))
                ? null
                : await Task.FromResult(_data[id]).ConfigureAwait(false);

        /// <summary>
        /// Check if the given blob is stored.
        /// </summary>
        public async Task<bool> HasBlob(Guid id)
            => await Task.FromResult(_data.ContainsKey(id)).ConfigureAwait(false);
    }
}
