using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Messages.Storage
{
    /// <summary>
    /// Stores messages in memory.
    /// </summary>
    public class HCMemoryMessageStore : IHCMessageStorage
    {
        /// <summary>
        /// Max number of unique inboxes to keep in memory.
        /// </summary>
        public int MaxLatestInboxCount { get; set; } = 100;

        /// <summary>
        /// Max number of latest messages to keep per inbox.
        /// </summary>
        public int MaxLatestMessageCountPerInbox { get; set; } = 100;

        private readonly List<Inbox> _inboxes = new List<Inbox>();
        private readonly Dictionary<string, Inbox> _inboxesById = new Dictionary<string, Inbox>();
        private readonly object _cleanupLock = new object();

        /// <summary>
        /// Add a new message to the given inbox.
        /// </summary>
        public Task StoreMessageAsync(string inboxId, IHCMessageItem message)
        {
            lock (_inboxesById)
            {
                if (!_inboxesById.ContainsKey(inboxId))
                {
                    var newInbox = new Inbox()
                    {
                        Id = inboxId,
                        Items = new List<IHCMessageItem>()
                    };
                    _inboxes.Insert(0, newInbox);
                    _inboxesById[inboxId] = newInbox;
                }

                var inbox = _inboxesById[inboxId];
                inbox.Items.Insert(0, message);
            }

            Cleanup();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the latest messages from the given inbox.
        /// </summary>
        public async Task<HCDataWithTotalCount<IEnumerable<IHCMessageItem>>> GetLatestMessagesAsync(string inboxId, int pageSize, int pageIndex)
        {
            HCDataWithTotalCount<IEnumerable<IHCMessageItem>> result = null;

            lock (_inboxesById)
            {
                if (!_inboxesById.ContainsKey(inboxId))
                {
                    result = new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>
                    {
                        Data = Enumerable.Empty<IHCMessageItem>()
                    };
                }
                else
                {
                    var inbox = _inboxesById[inboxId];
                    var items = inbox.Items.Skip(pageIndex * pageSize).Take(pageSize);
                    result = new HCDataWithTotalCount<IEnumerable<IHCMessageItem>>
                    {
                        Data = items,
                        TotalCount = inbox.Items.Count
                    };
                }
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Get a single messages from the given inbox.
        /// </summary>
        public async Task<IHCMessageItem> GetMessageAsync(string inboxId, string messageId)
        {
            IHCMessageItem match = null;
            lock (_inboxesById)
            {
                if (_inboxesById.ContainsKey(inboxId))
                {
                    var inbox = _inboxesById[inboxId];
                    match = inbox.Items.FirstOrDefault(x => x.Id == messageId);
                }
            }
            return await Task.FromResult(match);
        }

        /// <summary>
        /// Delete a message with the given id.
        /// </summary>
        public async Task<bool> DeleteMessageAsync(string inboxId, string messageId)
        {
            lock (_inboxesById)
            {
                if (!_inboxesById.ContainsKey(inboxId)) {
                    return false;
                }
                var inbox = _inboxesById[inboxId];
                inbox.Items.RemoveAll(x => x.Id == messageId);
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Delete a whole inbox with the given id.
        /// </summary>
        public async Task<bool> DeleteInboxAsync(string inboxId)
        {
            lock(_inboxesById)
            {
                if (!_inboxesById.ContainsKey(inboxId))
                {
                    return false;
                }

                _inboxesById.Remove(inboxId);
                _inboxes.RemoveAll(x => x.Id == inboxId);
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Delete all stored data.
        /// </summary>
        public Task DeleteAllDataAsync()
        {
            lock (_inboxesById)
            {
                _inboxesById.Clear();
                _inboxes.Clear();
            }
            return Task.CompletedTask;
        }

        private void Cleanup()
        {
            lock (_cleanupLock)
            {
                if (_inboxes.Count > MaxLatestInboxCount)
                {
                    var toRemoveCount = _inboxes.Count - MaxLatestInboxCount;
                    for (int i = 0; i < toRemoveCount; i++)
                    {
                        var inbox = _inboxes[_inboxes.Count - 1];
                        _inboxes.RemoveAt(_inboxes.Count - 1);
                        _inboxesById.Remove(inbox.Id);
                    }
                }

                foreach(var inbox in _inboxes)
                {
                    if (inbox.Items.Count > MaxLatestMessageCountPerInbox)
                    {
                        var toRemoveCount = inbox.Items.Count - MaxLatestMessageCountPerInbox;
                        for (int i = 0; i < toRemoveCount; i++)
                        {
                            inbox.Items.RemoveAt(inbox.Items.Count - 1);
                        }
                    }
                }
            }
        }

        private class Inbox
        {
            public string Id { get; set; }
            public List<IHCMessageItem> Items { get; set; }
        }
    }
}
