using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using QoDL.Toolkit.Core.Util.Storage;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Episerver.Storage
{
    /// <summary>
    /// Stores <see cref="KnownEventDefinition"/>s.
    /// </summary>
    public class TKEpiserverBlobEventSinkKnownEventDefinitionsStorage
        : TKSingleBufferedDictionaryBlobStorageBase<TKEpiserverBlobEventSinkKnownEventDefinitionsStorage.TKEventSinkKnownEventDefinitionsBlobData, KnownEventDefinition, string>, IEventSinkKnownEventDefinitionsStorage
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("8832e8d5-b4d4-40b7-8c5e-4e436156b4fa");

        /// <summary>
        /// Defaults to the default provider if null.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Defaults to a hardcoded guid if null
        /// </summary>
        public Guid? ContainerId { get; set; }

        /// <summary>
        /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
        /// </summary>
        protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

        /// <inheritdoc />
        protected override string CacheKey => $"__tk_{ContainerIdWithFallback}";

        private readonly TKEpiserverBlobHelper<TKEventSinkKnownEventDefinitionsBlobData> _blobHelper;

        /// <summary>
        /// Stores <see cref="KnownEventDefinition"/>s.
        /// </summary>
        public TKEpiserverBlobEventSinkKnownEventDefinitionsStorage(IBlobFactory blobFactory, ITKCache cache)
            : base(cache)
        {
            SupportsMaxItemAge = false;
            SupportsExpirationTime = false;
            _blobHelper = new TKEpiserverBlobHelper<TKEventSinkKnownEventDefinitionsBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        #region IEventSinkKnownEventDefinitionsStorage Implementation
        /// <inheritdoc />
        public IEnumerable<KnownEventDefinition> GetDefinitions() => GetItems();

        /// <inheritdoc />
        public KnownEventDefinition InsertDefinition(KnownEventDefinition definition)
        {
            InsertItemBuffered(definition, definition.EventId);
            return definition;
        }

        /// <inheritdoc />
        public KnownEventDefinition UpdateDefinition(KnownEventDefinition definition)
        {
            InsertItemBuffered(definition, definition.EventId, isUpdate: true);
            return definition;
        }

        /// <inheritdoc />
        public void DeleteDefinition(string eventId) => RemoveItem(eventId);

        /// <inheritdoc />
        public void DeleteDefinitions() => RemoveAllItems();
        #endregion

        /// <inheritdoc />
        protected override TKEventSinkKnownEventDefinitionsBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(TKEventSinkKnownEventDefinitionsBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class TKEventSinkKnownEventDefinitionsBlobData : IBufferedBlobDictionaryStorageData
        {
            /// <summary>
            /// All stored definitions.
            /// </summary>
            public Dictionary<string, KnownEventDefinition> Items { get; set; } = new();
        }
    }
}
