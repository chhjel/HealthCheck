using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores messages in a json file.
    /// </summary>
    public class HCFlatFileMessageStore : IHCMessageStorage
    {
        /// <summary>
        /// Max number of messages to keep per inbox.
        /// </summary>
        public int MaxMessagesStoredPerInbox { get; set; }

        private readonly object _storeLock = new object();
        private readonly Dictionary<string, SimpleDataStoreWithId<HCDefaultMessageItem, string>> _inboxStores
            = new Dictionary<string, SimpleDataStoreWithId<HCDefaultMessageItem, string>>();
        private readonly string _baseFilepath;

        /// <summary>
        /// Create a new <see cref="HCFlatFileMessageStore"/> with the given file path.
        /// </summary>
        /// <param name="folderPath">Folder to store the data in one file per unique inbox id.</param>
        public HCFlatFileMessageStore(string folderPath)
        {
            _baseFilepath = folderPath;
        }

        private bool HasStoreFor(string inboxId)
        {
            lock (_storeLock)
            {
                return _inboxStores.ContainsKey(inboxId);
            }
        }

        private SimpleDataStoreWithId<HCDefaultMessageItem, string> GetOrCreateStoreFor(string inboxId)
        {
            lock (_storeLock)
            {
                if (_inboxStores.ContainsKey(inboxId))
                {
                    return _inboxStores[inboxId];
                }

                var filepath = Path.Combine(_baseFilepath, $"HCMessages_{inboxId}.json");
                var store = new SimpleDataStoreWithId<HCDefaultMessageItem, string>(
                    filepath,
                    serializer: new Func<HCDefaultMessageItem, string>((e) => JsonConvert.SerializeObject(e)),
                    deserializer: new Func<string, HCDefaultMessageItem>((row) => JsonConvert.DeserializeObject<HCDefaultMessageItem>(row)),
                    idSelector: (e) => e.Id,
                    idSetter: (e, id) => e.Id = id,
                    nextIdFactory: (events, e) => Guid.NewGuid().ToString()
                );
                _inboxStores[inboxId] = store;

                return store;
            }
        }

        /// <summary>
        /// Delete all stored data.
        /// </summary>
        public async Task DeleteAllDataAsync()
        {
            foreach (var kvp in _inboxStores)
            {
                await kvp.Value.ClearDataAsync();
            }
        }

        /// <summary>
        /// Delete a whole inbox with the given id.
        /// </summary>
        public async Task<bool> DeleteInboxAsync(string inboxId)
        {
            if (!HasStoreFor(inboxId))
            {
                return false;
            }

            var store = GetOrCreateStoreFor(inboxId);
            await store.ClearDataAsync();
            return true;
        }

        /// <summary>
        /// Delete a message with the given id.
        /// </summary>
        public Task<bool> DeleteMessageAsync(string inboxId, string messageId)
        {
            if (!HasStoreFor(inboxId))
            {
                return Task.FromResult(false);
            }

            var store = GetOrCreateStoreFor(inboxId);
            store.DeleteItem(messageId);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Get the latest messages from the given inbox.
        /// </summary>
        public async Task<HCDataWithTotalCount<IEnumerable<IHCMessageItem>>> GetLatestMessagesAsync(string inboxId, int pageSize, int pageIndex)
        {
            var result = new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>() { Data = Enumerable.Empty<IHCMessageItem>() };
            if (!HasStoreFor(inboxId))
            {
                return result;
            }

            var store = GetOrCreateStoreFor(inboxId);
            var allItems = store.GetEnumerable().ToArray();
            var items = allItems.Skip(pageIndex * pageSize).Take(pageSize);
            result.Data = items;
            result.TotalCount = allItems.Length;
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Get a single messages from the given inbox.
        /// </summary>
        public async Task<IHCMessageItem> GetMessageAsync(string inboxId, string messageId)
        {
            if (!HasStoreFor(inboxId))
            {
                return await Task.FromResult<IHCMessageItem>(null);
            }

            var store = GetOrCreateStoreFor(inboxId);
            return store.GetItem(messageId);
        }

        /// <summary>
        /// Add a new message to the given inbox.
        /// </summary>
        public Task StoreMessageAsync(string inboxId, IHCMessageItem message)
        {
            if (!(message is HCDefaultMessageItem hcMessage))
            {
                throw new ArgumentException($"Message parameter must be of type {nameof(HCDefaultMessageItem)}.", nameof(message));
            }

            // todo: store in memory as well & discard?
            // => refactor request history into a FlatfileMemoryStorage for limited amount of data w/ delayed saving & limits.

            var store = GetOrCreateStoreFor(inboxId);
            store.InsertItem(hcMessage);
            return Task.CompletedTask;
        }
    }
}
