using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores messages in a json file.
    /// </summary>
    public class HCFlatFileMessageStore : IHCMessageStorage
    {
        /// <summary>
        /// If disabled no messages will be stored.
        /// <para>Null value/exception = false.</para>
        /// </summary>
        public Func<bool> EnableStoringMessages { get; set; } = () => true;

        /// <summary>
        /// Max number of messages to keep per inbox.
        /// </summary>
        public int MaxMessagesStoredPerInbox { get; set; } = 200;

        /// <summary>
        /// Max duration to keep data for.
        /// <para>Defaults to 7 days.</para>
        /// </summary>
        public TimeSpan MaxMessageAge { get; set; } = TimeSpan.FromDays(7);

        private readonly object _storeLock = new object();
        private readonly Dictionary<string, SimpleCachedDataStore<HCDefaultMessageItem>> _inboxStores
            = new Dictionary<string, SimpleCachedDataStore<HCDefaultMessageItem>>();
        private readonly string _baseFilepath;
        private readonly IJsonSerializer _jsonSerializer = new NewtonsoftJsonSerializer();

        /// <summary>
        /// Create a new <see cref="HCFlatFileMessageStore"/> with the given file path.
        /// </summary>
        /// <param name="folderPath">Folder to store the data in one file per unique inbox id.</param>
        public HCFlatFileMessageStore(string folderPath)
        {
            _baseFilepath = folderPath;
        }

        /// <summary>
        /// If disabled no messages will be stored.
        /// <para>Null value/exception = false.</para>
        /// </summary>
        public HCFlatFileMessageStore SetIsEnabled(Func<bool> isEnabledFunc)
        {
            EnableStoringMessages = isEnabledFunc;
            return this;
        }

        private bool HasStoreFor(string inboxId)
        {
            lock (_storeLock)
            {
                return _inboxStores.ContainsKey(inboxId);
            }
        }

        private SimpleCachedDataStore<HCDefaultMessageItem> GetOrCreateStoreFor(string inboxId)
        {
            lock (_storeLock)
            {
                if (_inboxStores.ContainsKey(inboxId))
                {
                    return _inboxStores[inboxId];
                }

                var filepath = Path.Combine(_baseFilepath, IOUtils.SanitizeFilename($"hc_messages_{inboxId}.json"));
                var store = new SimpleCachedDataStore<HCDefaultMessageItem>(_jsonSerializer, filepath)
                {
                    MaxMemoryItemCount = MaxMessagesStoredPerInbox,
                    MaxStoredItemCount = MaxMessagesStoredPerInbox,
                    MaxDataAge = MaxMessageAge
                };
                _inboxStores[inboxId] = store;

                return store;
            }
        }

        /// <summary>
        /// Delete all stored data.
        /// </summary>
        public void DeleteAllData()
        {
            foreach (var kvp in _inboxStores)
            {
                kvp.Value.Clear();
            }
        }

        /// <summary>
        /// Delete a whole inbox with the given id.
        /// </summary>
        public bool DeleteInbox(string inboxId)
        {
            if (!HasStoreFor(inboxId))
            {
                return false;
            }

            var store = GetOrCreateStoreFor(inboxId);
            store.Clear();
            return true;
        }

        /// <summary>
        /// Delete a message with the given id.
        /// </summary>
        public bool DeleteMessage(string inboxId, string messageId)
        {
            if (!HasStoreFor(inboxId))
            {
                return false;
            }

            var store = GetOrCreateStoreFor(inboxId);
            store.RemoveAll(x => x.Id == messageId);
            return true;
        }

        /// <summary>
        /// Get the latest messages from the given inbox.
        /// </summary>
        public HCDataWithTotalCount<IEnumerable<IHCMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex)
        {
            var result = new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>() { Data = Enumerable.Empty<IHCMessageItem>() };

            if (!HasStoreFor(inboxId))
            {
                return result;
            }

            var store = GetOrCreateStoreFor(inboxId);
            var allItems = store.GetItems();
            var items = allItems.Skip(pageIndex * pageSize).Take(pageSize);
            result.Data = items;
            result.TotalCount = allItems.Count();
            return result;
        }

        /// <summary>
        /// Get a single messages from the given inbox.
        /// </summary>
        public IHCMessageItem GetMessage(string inboxId, string messageId)
        {
            if (!HasStoreFor(inboxId))
            {
                return null;
            }

            var store = GetOrCreateStoreFor(inboxId);
            return store.GetItems().FirstOrDefault(x => x.Id == messageId);
        }

        /// <summary>
        /// Add a new message to the given inbox.
        /// </summary>
        public void StoreMessage(string inboxId, IHCMessageItem message)
        {
            if (!EnableStoringMessagesInternal())
            {
                return;
            }

            if (!(message is HCDefaultMessageItem hcMessage))
            {
                throw new ArgumentException($"Message parameter must be of type {nameof(HCDefaultMessageItem)}.", nameof(message));
            }

            var store = GetOrCreateStoreFor(inboxId);
            store.AddItem(hcMessage);
        }

        internal bool EnableStoringMessagesInternal()
        {
            try
            {
                if (EnableStoringMessages?.Invoke() != true)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            return true;
        }
    }
}
