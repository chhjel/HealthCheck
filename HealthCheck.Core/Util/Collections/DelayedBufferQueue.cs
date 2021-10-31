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
        /// Performs the given update on all items matchin the condition.
        /// </summary>
        public virtual void UpdateQueuedItem(Func<T, bool> condition, Action<T> updateAction)
        {
            lock (BufferQueue)
            {
                foreach (var item in BufferQueue)
                {
                    if (condition(item))
                    {
                        updateAction(item);
                    }
                }
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

                    InvokeCallbackAndClearQueue();
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
