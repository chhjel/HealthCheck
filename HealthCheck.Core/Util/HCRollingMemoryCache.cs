using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Caches up to a given number items.
    /// </summary>
    public class HCRollingMemoryCache<TKey, TValue>
    {
        /// <summary>
        /// Defaults to 1000.
        /// </summary>
        public int MaxCount { get; set; } = 1000;

        private static readonly Dictionary<TKey, TValue> _cache = new();

        /// <summary>
        /// Get or create a value.
        /// </summary>
        public TValue GetOrCreate(TKey key, Func<TValue> factory)
        {
            if (key == null) return default;

            lock (_cache)
            {
                if (!_cache.ContainsKey(key))
                {
                    if (_cache.Count > MaxCount)
                    {
                        _cache.Clear();
                    }

                    _cache[key] = factory();
                }
                return _cache[key];
            }
        }
    }
}
