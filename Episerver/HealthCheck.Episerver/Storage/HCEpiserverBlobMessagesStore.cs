using EPiServer.Framework.Blobs;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using HealthCheck.Episerver.Storage.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores messages in blob storage.
    /// </summary>
    public class HCEpiserverBlobMessagesStore
        : HCEpiserverSingleBufferedMultiListBlobStorageBase<HCEpiserverBlobMessagesStore.HCMessagesBlobData, IHCMessageItem, string>, IHCMessageStorage
    {
        /// <inheritdoc />
        protected override Guid DefaultContainerId => Guid.Parse("d22175a0-28b2-4f5f-9acd-5c135666f08e");

        /// <summary>
        /// Stores messages in blob storage.
        /// </summary>
        public HCEpiserverBlobMessagesStore(IBlobFactory blobFactory, IMemoryCache cache)
            : base(blobFactory, cache)
        {}

        /// <inheritdoc />
        public HCDataWithTotalCount<IEnumerable<IHCMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex)
        {
            var data = GetBlobData();

            var totalCount = 0;
            IEnumerable<IHCMessageItem> items = null;
            if (data.Lists.ContainsKey(inboxId))
            {
                totalCount = data.Lists[inboxId].Count;
                items = data.Lists[inboxId]
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
            }

            return new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>
            {
                TotalCount = totalCount,
                Data = items ?? Enumerable.Empty<IHCMessageItem>()
            };
        }

        /// <inheritdoc />
        public IHCMessageItem GetMessage(string inboxId, string messageId)
        {
            var data = GetBlobData();
            if (!data.Lists.ContainsKey(inboxId))
            {
                return null;
            }
            return data.Lists[inboxId].FirstOrDefault(x => x.Id == messageId);
        }

        /// <inheritdoc />
        public void StoreMessage(string inboxId, IHCMessageItem message)
            => InsertItemBuffered(message, inboxId);

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

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCMessagesBlobData : IBufferedBlobMultiListStorageData
        {
            /// <inheritdoc />
            public Dictionary<string, List<IHCMessageItem>> Lists { get; set; } = new Dictionary<string, List<IHCMessageItem>>();
        }
    }
}
