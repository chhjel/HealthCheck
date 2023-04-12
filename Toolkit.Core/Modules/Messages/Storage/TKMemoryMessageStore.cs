using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Messages.Storage;

/// <summary>
/// Stores messages in memory only.
/// </summary>
public class TKMemoryMessageStore : ITKMessageStorage
{
    /// <summary>
    /// If disabled no messages will be stored.
    /// <para>Null value/exception = false.</para>
    /// </summary>
    public Func<bool> EnableStoringMessages { get; set; } = () => true;

    /// <summary>
    /// Max number of unique inboxes to keep in memory.
    /// </summary>
    public int MaxLatestInboxCount { get; set; } = 100;

    /// <summary>
    /// Max number of latest messages to keep per inbox.
    /// </summary>
    public int MaxLatestMessageCountPerInbox { get; set; } = 100;

    private readonly List<Inbox> _inboxes = new();
    private readonly Dictionary<string, Inbox> _inboxesById = new();
    private readonly object _cleanupLock = new();

    /// <summary>
    /// If disabled no messages will be stored.
    /// <para>Null value/exception = false.</para>
    /// </summary>
    public TKMemoryMessageStore SetIsEnabled(Func<bool> isEnabledFunc)
    {
        EnableStoringMessages = isEnabledFunc;
        return this;
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

        lock (_inboxesById)
        {
            if (!_inboxesById.ContainsKey(inboxId))
            {
                var newInbox = new Inbox()
                {
                    Id = inboxId,
                    Items = new List<ITKMessageItem>()
                };
                _inboxes.Insert(0, newInbox);
                _inboxesById[inboxId] = newInbox;
            }

            var inbox = _inboxesById[inboxId];
            inbox.Items.Insert(0, message);
        }

        Cleanup();
    }

    /// <summary>
    /// Get the latest messages from the given inbox.
    /// </summary>
    public TKDataWithTotalCount<IEnumerable<ITKMessageItem>> GetLatestMessages(string inboxId, int pageSize, int pageIndex)
    {
        TKDataWithTotalCount<IEnumerable<ITKMessageItem>> result;
        
        lock (_inboxesById)
        {
            if (!_inboxesById.ContainsKey(inboxId))
            {
                result = new TKDataWithTotalCount<IEnumerable<ITKMessageItem>>
                {
                    Data = Enumerable.Empty<ITKMessageItem>()
                };
            }
            else
            {
                var inbox = _inboxesById[inboxId];
                var items = inbox.Items.Skip(pageIndex * pageSize).Take(pageSize);
                result = new TKDataWithTotalCount<IEnumerable<ITKMessageItem>>
                {
                    Data = items,
                    TotalCount = inbox.Items.Count
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Get a single messages from the given inbox.
    /// </summary>
    public ITKMessageItem GetMessage(string inboxId, string messageId)
    {
        lock (_inboxesById)
        {
            if (_inboxesById.ContainsKey(inboxId))
            {
                var inbox = _inboxesById[inboxId];
                return inbox.Items.FirstOrDefault(x => x.Id == messageId);
            }
        }
        return null;
    }

    /// <summary>
    /// Delete a message with the given id.
    /// </summary>
    public bool DeleteMessage(string inboxId, string messageId)
    {
        lock (_inboxesById)
        {
            if (!_inboxesById.ContainsKey(inboxId)) {
                return false;
            }
            var inbox = _inboxesById[inboxId];
            inbox.Items.RemoveAll(x => x.Id == messageId);
        }
        return true;
    }

    /// <summary>
    /// Delete a whole inbox with the given id.
    /// </summary>
    public bool DeleteInbox(string inboxId)
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
        return true;
    }

    /// <summary>
    /// Delete all stored data.
    /// </summary>
    public void DeleteAllData()
    {
        lock (_inboxesById)
        {
            _inboxesById.Clear();
            _inboxes.Clear();
        }
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

    private class Inbox
    {
        public string Id { get; set; }
        public List<ITKMessageItem> Items { get; set; }
    }
}
