using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Util
{
    internal class SimpleMemoryCache
    {
        public int MaxCount { get; set; } = 10000;

        private readonly Dictionary<string, SimpleMemoryCacheItem> _items = new Dictionary<string, SimpleMemoryCacheItem>();

        private struct SimpleMemoryCacheItem {
            public DateTime ExpiresAt;
            public object Value;
        }

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

        public void SetValue<T>(string key, T value, DateTime expiresAt)
        {
            lock (_items)
            {
                if (_items.Count >= MaxCount)
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

        public void ClearAll()
        {
            lock (_items)
            {
                _items.Clear();
            }
        }

        private void RemoveExpired()
        {
            var keys = _items.Keys;
            foreach(var key in keys)
            {
                if (_items[key].ExpiresAt <= DateTime.Now)
                {
                    _items.Remove(key);
                }
            }
        }
    }
}
