using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.WebUI.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QoDL.Toolkit.WebUI.Services
{
    /// <summary>
    /// Stores messages in a json file.
    /// </summary>
    public class TKFlatFileMessageStore : ITKMessageStorage
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

        private readonly object _storeLock = new();
        private readonly Dictionary<string, TKSimpleCachedDataStore<TKDefaultMessageItem>> _inboxStores
            = new();
        private readonly string _baseFilepath;
        private readonly IJsonSerializer _jsonSerializer = new NewtonsoftJsonSerializer();

        /// <summary>
        /// Create a new <see cref="TKFlatFileMessageStore"/> with the given file path.
        /// </summary>
        /// <param name="folderPath">Folder to store the data in one file per unique inbox id.</param>
        public TKFlatFileMessageStore(string folderPath)
        {
            _baseFilepath = folderPath;
        }

        /// <summary>
        /// If disabled no messages will be stored.
        /// <para>Null value/exception = false.</para>
        /// </summary>
        public TKFlatFileMessageStore SetIsEnabled(Func<bool> isEnabledFunc)
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

        private TKSimpleCachedDataStore<TKDefaultMessageItem> GetOrCreateStoreFor(string inboxId)
        {
            lock (_storeLock)
            {
                if (_inboxStores.ContainsKey(inboxId))
                {
                    return _inboxStores[inboxId];
                }

                var filepath = Path.Combine(_baseFilepath, TKIOUtils.SanitizeFilename($"tk_messages_{inboxId}.json"));
                var store = new TKSimpleCachedDataStore<TKDefaultMessageItem>(_jsonSerializer, filepath)
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
        public TKDataWithTotalCount<IEnumerable<ITKMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex)
        {
            var result = new TKDataWithTotalCount<IEnumerable<ITKMessageItem>>() { Data = Enumerable.Empty<ITKMessageItem>() };
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
        public ITKMessageItem GetMessage(string inboxId, string messageId)
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
        public void StoreMessage(string inboxId, ITKMessageItem message)
        {
            if (!EnableStoringMessagesInternal())
            {
                return;
            }

            if (message is not TKDefaultMessageItem tkMessage)
            {
                throw new ArgumentException($"Message parameter must be of type {nameof(TKDefaultMessageItem)}.", nameof(message));
            }

            var store = GetOrCreateStoreFor(inboxId);
            store.AddItem(tkMessage);
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
