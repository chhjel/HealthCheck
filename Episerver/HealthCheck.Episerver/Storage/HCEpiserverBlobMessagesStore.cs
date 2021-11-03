using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores messages in blob storage.
    /// </summary>
    public class HCEpiserverBlobMessagesStore<TMessageModel>
        : HCSingleBufferedMultiListBlobStorageBase<HCEpiserverBlobMessagesStore<TMessageModel>.HCMessagesBlobData, TMessageModel, string>, IHCMessageStorage
        where TMessageModel: class, IHCMessageItem
    {
        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("882175a0-28b2-4f5f-9acd-5c135666f08e");

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
        protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

        private readonly HCEpiserverBlobHelper<HCMessagesBlobData> _blobHelper;

        /// <summary>
        /// Stores messages in blob storage.
        /// </summary>
        public HCEpiserverBlobMessagesStore(IBlobFactory blobFactory, IHCCache cache)
            : base(cache)
        {
            MaxBufferSize = 2500;
            _blobHelper = new HCEpiserverBlobHelper<HCMessagesBlobData>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
        }

        /// <inheritdoc />
        public HCDataWithTotalCount<IEnumerable<IHCMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex)
        {
            var allItems = GetItems(inboxId).ToArray();
            var totalCount = allItems.Length;
            var items = allItems
                .OfType<IHCMessageItem>()
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            return new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>
            {
                TotalCount = totalCount,
                Data = items ?? Enumerable.Empty<IHCMessageItem>()
            };
        }

        /// <inheritdoc />
        public IHCMessageItem GetMessage(string inboxId, string messageId)
        {
            var items = GetItems(inboxId);
            return items.FirstOrDefault(x => x.Id == messageId);
        }

        /// <inheritdoc />
        public void StoreMessage(string inboxId, IHCMessageItem message)
            => InsertItemBuffered(message as TMessageModel, id: message.Id, groupId: inboxId);

        /// <inheritdoc />
        public void DeleteAllData()
        {
            var data = GetBlobData();
            data.Lists.Clear();
            SaveBlobData(data);
        }

        /// <inheritdoc />
        public bool DeleteInbox(string inboxId)
        {
            var data = GetBlobData();
            if (data.Lists.ContainsKey(inboxId))
            {
                data.Lists.Remove(inboxId);
                SaveBlobData(data);
            }
            return true;
        }

        /// <inheritdoc />
        public bool DeleteMessage(string inboxId, string messageId)
        {
            var data = GetBlobData();
            if (data.Lists.ContainsKey(inboxId))
            {
                data.Lists[inboxId].RemoveAll(x => x.Id == messageId);
                SaveBlobData(data);
            }
            return true;
        }

        /// <inheritdoc />
        protected override HCMessagesBlobData RetrieveBlobData() => _blobHelper.RetrieveBlobData();

        /// <inheritdoc />
        protected override void StoreBlobData(HCMessagesBlobData data) => _blobHelper.StoreBlobData(data);

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCMessagesBlobData : IBufferedBlobMultiListStorageData
        {
            /// <inheritdoc />
            public Dictionary<string, List<TMessageModel>> Lists { get; set; } = new Dictionary<string, List<TMessageModel>>();
        }
    }
}
