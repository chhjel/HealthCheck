using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Util.Collections;

/// <summary>
/// Queues up additions either up to the given duration or max count, then invokes the callback and clears the queue.
/// <para>Is threadsafe and invokes callback async.</para>
/// </summary>
public class TKDelayedBufferDictionary<TId, T>
{
    /// <summary>
    /// If the queue size goes above this count the callback will be invoked, the queue cleared and duration reset.
    /// <para>Defaults to 100.</para>
    /// </summary>
    public int QueueSizeLimit { get; set; } = 1000;

    /// <summary>
    /// The max duration to wait before the callback will be invoked, the queue cleared and duration reset.
    /// <para>Each addition starts or extends an internal timer with this duration.</para>
    /// <para>Defaults to 5 seconds.</para>
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Retrieves id from an item.
    /// </summary>
    public Func<T, TId> IdGetter { get; }

    /// <summary>
    /// Invoked when either the given duration or max count is reached after adding an item.
    /// <para>Before this call the buffer will be cleared, a copy of the buffer is passed async to this action.</para>
    /// </summary>
    public Action<Dictionary<TId, T>> Callback { get; set; }

    /// <summary>
    /// Contains added items until the callback is invoked.
    /// </summary>
    protected Dictionary<TId, T> BufferQueue = new();

    private bool _isDelaying = false;

    /// <summary>
    /// Queues up additions either up to the given duration or max count, then invokes the callback and clears the queue.
    /// </summary>
    /// <param name="idGetter">Retrieves id from an item.</param>
    /// <param name="callback">Invoked when either the given duration or max count is reached after adding an item.</param>
    /// <param name="delay">The max duration to wait before the callback will be invoked, the queue cleared and duration reset. Default: 5 seconds.</param>
    /// <param name="queueSizeLimit">The max duration to wait before the callback will be invoked, the queue cleared and duration reset. Default: 1000.</param>
    public TKDelayedBufferDictionary(
        Func<T, TId> idGetter,
        Action<Dictionary<TId, T>> callback = null,TimeSpan? delay = null, int? queueSizeLimit = null)
    {
        IdGetter = idGetter;
        Callback = callback;
        if (delay != null)
        {
            Delay = delay.Value;
        }
        if (queueSizeLimit != null)
        {
            QueueSizeLimit = queueSizeLimit.Value;
        }
    }

    /// <summary>
    /// Deconstructor. Stores any buffered data before self destructing.
    /// </summary>
    ~TKDelayedBufferDictionary()
    {
        lock (BufferQueue)
        {
            if (!_isDelaying)
            {
                InvokeCallbackAndClearQueue();
            }
        }
    }

    /// <summary>
    /// Inserts an item into the buffer queue and ensures the callback timer is started.
    /// </summary>
    public virtual void Set(T item, Action<T, T> onUpdate = null)
    {
        lock (BufferQueue)
        {
            var id = IdGetter(item);
            if (onUpdate != null && id != null && BufferQueue.ContainsKey(id))
            {
                onUpdate?.Invoke(BufferQueue[id], item);
            }
            BufferQueue[id] = item;

            EnsureDelayedCallback();
        }
    }

    /// <summary>
    /// Inserts items into the buffer queue and ensures the callback timer is started.
    /// </summary>
    public virtual void Set(IEnumerable<T> items)
    {
        lock (BufferQueue)
        {
            foreach(var item in items)
            {
                BufferQueue[IdGetter(item)] = item;
            }

            EnsureDelayedCallback();
        }
    }

    /// <summary>
    /// Removes item with the given key.
    /// </summary>
    public void Remove(TId key)
    {
        lock (BufferQueue)
        {
            if (BufferQueue.ContainsKey(key))
            {
                BufferQueue.Remove(key);
            }
        }
    }

    /// <summary>
    /// Removes matching items.
    /// </summary>
    public void RemoveWhere(Func<T, bool> condition)
    {
        lock (BufferQueue)
        {
            foreach (var item in BufferQueue.ToArray())
            {
                if (condition(item.Value))
                {
                    BufferQueue.Remove(item.Key);
                }
            }
        }
    }

    /// <summary>
    /// Removes all items.
    /// </summary>
    public void Clear()
    {
        lock (BufferQueue)
        {
            BufferQueue.Clear();
        }
    }

    /// <summary>
    /// True if the queue contains a matching item.
    /// </summary>
    public bool ContainsKey(TId key)
    {
        lock (BufferQueue)
        {
            return BufferQueue.ContainsKey(key);
        }
    }

    /// <summary>
    /// Get a single item.
    /// </summary>
    public T Get(TId key)
    {
        lock (BufferQueue)
        {
            return BufferQueue[key];
        }
    }

    /// <summary>
    /// Get a single item.
    /// </summary>
    public bool TryGet(TId key, out T value)
    {
        lock (BufferQueue)
        {
            return BufferQueue.TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// Updates existing item if in queue, or inserts an updated item. 
    /// </summary>
    public bool UpdateQueuedItemOrInsertUpdated<TInnerItem>(
        TId id,
        Action<TInnerItem> updateAction, Action<T> updateBuffer,
        Func<TInnerItem> getExistingNonBufferedItem, Action<TInnerItem> addBuffered)
    {
        lock (BufferQueue)
        {
            // if bufferqueue has item => update it
            if (ContainsKey(id))
            {
                var item = BufferQueue[id];
                updateBuffer(item);
                return true;
            }

            // else add existing item to queue and update it
            var existingItem = getExistingNonBufferedItem();
            if (existingItem != null)
            {
                updateAction(existingItem);
                addBuffered(existingItem);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Retrieves all buffered items
    /// </summary>
    public virtual IEnumerable<T> GetBufferedItems()
    {
        lock (BufferQueue)
        {
            foreach (var item in BufferQueue)
            {
                yield return item.Value;
            }
        }
    }

    /// <summary>
    /// Skips waiting for the buffer and processes it at once.
    /// <para>Should not be needed for other things than unit testing.</para>
    /// </summary>
    public void ForceBufferCallback()
    {
        lock (BufferQueue)
        {
            InvokeCallbackAndClearQueue();
        }
    }

    private void EnsureDelayedCallback()
    {
        if (BufferQueue.Count >= QueueSizeLimit)
        {
            InvokeCallbackAndClearQueue();
        }
        else if (!_isDelaying)
        {
            _isDelaying = true;

            Task.Run(async () =>
            {
                await Task.Delay(Delay);
                _isDelaying = false;

                lock (BufferQueue)
                {
                    InvokeCallbackAndClearQueue();
                }
            });
        }
    }

    private void InvokeCallbackAndClearQueue()
    {
        if (Callback == null || !BufferQueue.Any())
        {
            return;
        }

        var bufferCopy = new Dictionary<TId, T>(BufferQueue);
        BufferQueue.Clear();
        Callback.Invoke(bufferCopy);
    }
}
