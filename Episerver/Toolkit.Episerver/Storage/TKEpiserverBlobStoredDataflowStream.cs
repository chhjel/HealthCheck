using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using System;

namespace QoDL.Toolkit.Episerver.Storage
{
    /// <summary>
    /// Datastream stored in an episerver blob.
    /// </summary>
    public abstract class TKEpiserverBlobStoredDataflowStream<TToolkitRoles, TEntry, TId> : StoredDataflowStream<TToolkitRoles, TEntry>
         where TEntry : IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Stores in epi blobs.
        /// <para>Shortcut to Store casted to the used type for configuration.</para>
        /// </summary>
        protected TKEpiserverBufferedBlobDataStoreWithEntryId<TEntry, TId> BlobStorage => Store as TKEpiserverBufferedBlobDataStoreWithEntryId<TEntry, TId>;

        /// <summary>
        /// Datastream stored in an episerver blob.
        /// </summary>
        protected TKEpiserverBlobStoredDataflowStream(IBlobFactory blobFactory, ITKCache cache, Guid containerId, Func<TEntry, TId> idSelector)
            : base(CreateStore(blobFactory, cache, containerId, idSelector))
        {
        }

        private static IDataStoreWithEntryId<TEntry> CreateStore(IBlobFactory blobFactory, ITKCache cache, Guid containerId, Func<TEntry, TId> idSelector)
            => new TKEpiserverBufferedBlobDataStoreWithEntryId<TEntry, TId>(blobFactory, cache, idSelector)
            {
                ContainerId = containerId
            };
    }
}
