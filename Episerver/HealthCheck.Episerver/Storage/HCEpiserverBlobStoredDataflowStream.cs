using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using System;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Datastream stored in an episerver blob.
    /// </summary>
    public abstract class HCEpiserverBlobStoredDataflowStream<THealthCheckRoles, TEntry, TId> : StoredDataflowStream<THealthCheckRoles, TEntry>
         where TEntry : IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Stores in epi blobs.
        /// <para>Shortcut to Store casted to the used type for configuration.</para>
        /// </summary>
        protected HCEpiserverBufferedBlobDataStoreWithEntryId<TEntry, TId> BlobStorage => Store as HCEpiserverBufferedBlobDataStoreWithEntryId<TEntry, TId>;

        /// <summary>
        /// Datastream stored in an episerver blob.
        /// </summary>
        protected HCEpiserverBlobStoredDataflowStream(IBlobFactory blobFactory, IHCCache cache, Guid containerId, Func<TEntry, TId> idSelector)
            : base(CreateStore(blobFactory, cache, containerId, idSelector))
        {
        }

        private static IDataStoreWithEntryId<TEntry> CreateStore(IBlobFactory blobFactory, IHCCache cache, Guid containerId, Func<TEntry, TId> idSelector)
            => new HCEpiserverBufferedBlobDataStoreWithEntryId<TEntry, TId>(blobFactory, cache, idSelector)
            {
                ContainerId = containerId
            };
    }
}
