using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util.Collections
{
    /// <summary>
    /// Queues up additions either up to the given duration or max count, then invokes the callback and clears the queue.
    /// <para>Is threadsafe and invokes callback async.</para>
    /// </summary>
    public class DelayedBufferQueue<T>
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
        /// Invoked when either the given duration or max count is reached after adding an item.
        /// <para>Before this call the buffer will be cleared, a copy of the buffer is passed async to this action.</para>
        /// </summary>
        public Action<Queue<T>> Callback { get; set; }

        /// <summary>
        /// Contains added items until the callback is invoked.
        /// </summary>
        protected Queue<T> BufferQueue = new();

        private bool _isDelaying = false;

        /// <summary>
        /// Queues up additions either up to the given duration or max count, then invokes the callback and clears the queue.
        /// </summary>
        /// <param name="callback">Invoked when either the given duration or max count is reached after adding an item.</param>
        /// <param name="delay">The max duration to wait before the callback will be invoked, the queue cleared and duration reset. Default: 5 seconds.</param>
        /// <param name="queueSizeLimit">The max duration to wait before the callback will be invoked, the queue cleared and duration reset. Default: 1000.</param>
        public DelayedBufferQueue(Action<Queue<T>> callback = null, TimeSpan? delay = null, int? queueSizeLimit = null)
        {
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
        ~DelayedBufferQueue()
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
        public virtual void Add(T item)
        {
            lock (BufferQueue)
            {
                BufferQueue.Enqueue(item);

                EnsureDelayedCallback();
            }
        }

        /// <summary>
        /// Inserts items into the buffer queue and ensures the callback timer is started.
        /// </summary>
        public virtual void Add(IEnumerable<T> items)
        {
            lock (BufferQueue)
            {
                foreach(var item in items)
                {
                    BufferQueue.Enqueue(item);
                }

                EnsureDelayedCallback();
            }
        }

        /// <summary>
        /// Removes all matching items from the buffer queue.
        /// </summary>
        public void RemoveMatching(Func<T, bool> filter)
        {
            lock (BufferQueue)
            {
                var newQueue = BufferQueue.Where(x => !filter(x)).ToArray();
                if (newQueue.Length != BufferQueue.Count)
                {
                    BufferQueue = new Queue<T>(newQueue);
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
        public bool Contains(Func<T, bool> predicate)
        {
            lock (BufferQueue)
            {
                return BufferQueue.Any(predicate);
            }
        }

        /// <summary>
        /// Updates existing item if in queue, or inserts an updated item. 
        /// </summary>
        public bool UpdateQueuedItemOrInsertUpdated<TInnerItem>(
            Func<T, bool> updateCondition, Action<TInnerItem> updateAction, Action<T> updateBuffer,
            Func<TInnerItem> getExistingNonBufferedItem, Action<TInnerItem> addBuffered)
        {
            lock (BufferQueue)
            {
                // if bufferqueue has item => update it
                foreach (var item in BufferQueue)
                {
                    if (updateCondition(item))
                    {
                        updateBuffer(item);
                        return true;
                    }
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
        /// Performs the given update on all items matchin the condition.
        /// </summary>
        public virtual int UpdateQueuedItem(Func<T, bool> condition, Action<T> updateAction)
        {
            lock (BufferQueue)
            {
                var count = 0;
                foreach (var item in BufferQueue)
                {
                    if (condition(item))
                    {
                        updateAction(item);
                        count++;
                    }
                }
                return count;
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
                    yield return item;
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

            var bufferCopy = new Queue<T>(BufferQueue);
            BufferQueue.Clear();
            Callback.Invoke(bufferCopy);
        }
    }
}
