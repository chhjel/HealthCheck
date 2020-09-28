using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Simple in-memory cache.
    /// </summary>
    public class SimpleMemoryCache<T> : SimpleMemoryCache
    {
        /// <summary>
        /// Duration used for the indexer.
        /// </summary>
        public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Get/set cached value with <see cref="DefaultDuration"/>.
        /// </summary>
        public T this[string key]
        {
            get => GetValue<T>(key);
            set => SetValue(key, value, DefaultDuration);
        }
    }

    /// <summary>
    /// Simple in-memory cache.
    /// </summary>
    public class SimpleMemoryCache
    {
        /// <summary>
        /// Optional max entry count to store. If the limit is reached all entries are cleared.
        /// </summary>
        public int? MaxCount { get; set; }

        private readonly Dictionary<string, SimpleMemoryCacheItem> _items = new Dictionary<string, SimpleMemoryCacheItem>();
        private DateTime? _lastCleanupTime = null;

        private struct SimpleMemoryCacheItem {
            public DateTime ExpiresAt;
            public object Value;
        }

        /// <summary>
        /// Get a cached value casted to the given type.
        /// </summary>
        public T GetValue<T>(string key, T fallback = default)
        {
            lock(_items)
            {
                RemoveExpired();

                return _items.ContainsKey(key) 
                    ? (T)_items[key].Value
                    : fallback;
            }
        }

        /// <summary>
        /// Check if a key is cached.
        /// </summary>
        public bool ContainsKey(string key)
        {
            lock (_items)
            {
                RemoveExpired();

                return _items.ContainsKey(key);
            }
        }

        /// <summary>
        /// Cache a boolean with the value true for the given duration.
        /// </summary>
        public void SetFlag(string key, TimeSpan expiresIn)
            => SetValue<bool>(key, true, expiresIn);

        /// <summary>
        /// Cache a boolean with the value true for the given duration.
        /// </summary>
        public void SetFlag(string key, DateTime expiresAt)
            => SetValue<bool>(key, true, expiresAt);

        /// <summary>
        /// Check if a boolean with the value true is cached for the given key.
        /// </summary>
        public bool HasFlag(string key)
            => GetValue<bool>(key);

        /// <summary>
        /// Set a cached value of the given type.
        /// </summary>
        public void SetValue<T>(string key, T value, TimeSpan expiresIn)
            => SetValue<T>(key, value, DateTime.Now + expiresIn);

        /// <summary>
        /// Set a cached value of the given type.
        /// </summary>
        public void SetValue<T>(string key, T value, DateTime expiresAt)
        {
            lock (_items)
            {
                if (MaxCount != null && _items.Count >= MaxCount)
                {
                    _items.Clear();
                }

                RemoveExpired();

                _items[key] = new SimpleMemoryCacheItem
                {
                    Value = value,
                    ExpiresAt = expiresAt
                };
            }
        }

        /// <summary>
        /// Clear the given cached entry.
        /// </summary>
        public void ClearKey(string key)
        {
            lock (_items)
            {
                if (_items.ContainsKey(key))
                {
                    _items.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clear the whole cache.
        /// </summary>
        public void ClearAll()
        {
            lock (_items)
            {
                _items.Clear();
            }
        }

        /// <summary>
        /// Remove all expired entries.
        /// <para>This is invoked automatically, but can also be used manually for special cases.</para>
        /// </summary>
        public void RemoveExpired(TimeSpan? onlyIfLongerThanThisSinceLastCleanup = null)
        {
            lock (_items)
            {
                if (onlyIfLongerThanThisSinceLastCleanup != null && _lastCleanupTime != null
                    && (DateTime.Now - _lastCleanupTime.Value) <= onlyIfLongerThanThisSinceLastCleanup.Value)
                {
                    return;
                }

                _lastCleanupTime = DateTime.Now;
                var keys = _items;
                for (int i=0;i<keys.Count;i++)
                {
                    var key = keys.ElementAt(i).Key;
                    if (_items[key].ExpiresAt <= DateTime.Now)
                    {
                        _items.Remove(key);
                        i--;
                    }
                }
            }
        }
    }
}
