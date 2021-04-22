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
        : HCEpiserverSingleBufferedListBlobStorageBase<HCEpiserverBlobMessagesStore.HCMessagesBlobData, KeyValuePair<string, IHCMessageItem>>, IHCMessageStorage
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

            return new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>
            {
                TotalCount = data.Items.Count,
                Data = data.Items
                    .Where(x => x.Key == inboxId)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .Select(x => x.Value)
            };
        }

        /// <inheritdoc />
        public IHCMessageItem GetMessage(string inboxId, string messageId)
        {
            var data = GetBlobData();
            return data.Items.FirstOrDefault(x => x.Key == inboxId && x.Value.Id == messageId).Value;
        }

        /// <inheritdoc />
        public void StoreMessage(string inboxId, IHCMessageItem message)
            => InsertItemBuffered(new KeyValuePair<string, IHCMessageItem>(inboxId, message));

        /// <inheritdoc />
        public void DeleteAllData()
        {
            var data = GetBlobData();
            data.Items.Clear();
            SaveBlobData(data);
        }

        /// <inheritdoc />
        public bool DeleteInbox(string inboxId)
        {
            var data = GetBlobData();
            data.Items.RemoveAll(x => x.Key == inboxId);
            SaveBlobData(data);
            return true;
        }

        /// <inheritdoc />
        public bool DeleteMessage(string inboxId, string messageId)
        {
            var data = GetBlobData();
            data.Items.RemoveAll(x => x.Key == inboxId && x.Value.Id == messageId);
            SaveBlobData(data);
            return true;
        }

        /// <summary>
        /// Model stored in blob storage.
        /// </summary>
        public class HCMessagesBlobData : IBufferedBlobListStorageData
        {
            /// <inheritdoc />
            public List<KeyValuePair<string, IHCMessageItem>> Items { get; set; } = new List<KeyValuePair<string, IHCMessageItem>>();
        }
    }
}
