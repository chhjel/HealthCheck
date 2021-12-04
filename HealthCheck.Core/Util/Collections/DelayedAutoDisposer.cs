using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util.Collections
{
    /// <summary>
    /// Disposes the set item after a given time and re-initializes it.
    /// </summary>
    public class DelayedAutoDisposer<T>
        where T : IDisposable, new()
    {
        /// <summary>
        /// Retrieves value and ensures delayed disposal is running.
        /// </summary>
        public T Value
        {
            get
            {
                lock (_value)
                {
                    EnsureTimerStarted();
                }
                return _value;
            }
        }

        /// <summary>
        /// How long to wait for disposal after value has been set.
        /// </summary>
        public TimeSpan Delay { get; set; }

        private bool _isDelaying = false;
        private T _value;

        /// <summary>
        /// Disposes the set item after a given time and re-initializes it.
        /// </summary>
        public DelayedAutoDisposer(TimeSpan delay)
        {
            Delay = delay;
            _value = new T();
        }

        /// <summary>
        /// Deconstructor. Stores any buffered data before self destructing.
        /// </summary>
        ~DelayedAutoDisposer()
        {
            lock (_value)
            {
                if (!_isDelaying)
                {
                    DisposeAndResetValue();
                }
            }
        }

        private void EnsureTimerStarted()
        {
            if (!_isDelaying)
            {
                _isDelaying = true;

                Task.Run(async () =>
                {
                    await Task.Delay(Delay);
                    _isDelaying = false;

                    lock (_value)
                    {
                        DisposeAndResetValue();
                    }
                });
            }
        }

        private void DisposeAndResetValue()
        {
            lock (_value)
            {
                _value?.Dispose();
                _value = new T();
            }
        }
    }
}
