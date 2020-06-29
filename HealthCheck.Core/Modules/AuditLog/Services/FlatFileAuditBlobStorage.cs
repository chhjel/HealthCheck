using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Util;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.AuditLog.Services
{
    /// <summary>
    /// Stores event blobs to separate files in a given folder.
    /// </summary>
    public class FlatFileAuditBlobStorage : IAuditBlobStorage
    {
        private SimpleBlobStore Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileAuditBlobStorage"/> with the given file path.
        /// </summary>
        /// <param name="blobFolder">Path to the file where data will be stored.</param>
        /// <param name="maxEventAge">Max age of entries before they can become deleted. Leave at null to disable cleanup.</param>
        /// <param name="delayFirstCleanup">Delay first cleanup by the lowest of 4 hours or max event age.</param>
        public FlatFileAuditBlobStorage(string blobFolder,
            TimeSpan? maxEventAge = null,
            bool delayFirstCleanup = true)
        {
            Store = new SimpleBlobStore(blobFolder);

            if (maxEventAge != null)
            {
                var minimumCleanupInterval = TimeSpan.FromHours(4);
                Store.RetentionOptions = new SimpleBlobStoreRetentionOptions(
                    maxAge: maxEventAge.Value,
                    minimumCleanupInterval: (maxEventAge.Value < minimumCleanupInterval) ? maxEventAge.Value : minimumCleanupInterval,
                    delayFirstCleanup: delayFirstCleanup
                );
            }
        }

        /// <summary>
        /// Get an string blob object with the given id.
        /// </summary>
        public async Task<string> GetBlob(Guid id) => await Task.FromResult(Store.GetBlob(id));

        /// <summary>
        /// Returns true if a blob for the given id is found.
        /// </summary>
        public async Task<bool> HasBlob(Guid id) => await Task.FromResult(Store.HasBlob(id));

        /// <summary>
        /// Perform cleanup if retention options allow it.
        /// </summary>
        public async Task<Guid> StoreBlob(string data) => await Task.FromResult(Store.StoreBlob(data));
    }
}
