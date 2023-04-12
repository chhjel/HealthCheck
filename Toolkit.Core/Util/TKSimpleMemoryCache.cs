using QoDL.Toolkit.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Simple in-memory cache.
/// </summary>
public class TKSimpleMemoryCache<T> : TKSimpleMemoryCache
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
public class TKSimpleMemoryCache : ITKCache
{
    /// <summary>
    /// Optional max entry count to store. If the limit is reached all entries are cleared.
    /// </summary>
    public int? MaxCount { get; set; }

    private readonly Dictionary<string, SimpleMemoryCacheItem> _items = new();
    private DateTime? _lastCleanupTime = null;

    private struct SimpleMemoryCacheItem
    {
        public DateTime ExpiresAt;
        public object Value;
    }

    /// <summary>
    /// Get a cached value casted to the given type.
    /// </summary>
    public virtual T GetValue<T>(string key, T fallback = default)
    {
        lock (_items)
        {
            RemoveExpired();

            var value = _items.ContainsKey(key)
                ? _items[key].Value
                : fallback;

            if (value != null && !typeof(T).IsInstanceOfType(value))
            {
                return fallback;
            }
            return (T)value;
        }
    }

    /// <summary>
    /// Check if a key is cached.
    /// </summary>
    public virtual bool ContainsKey(string key)
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
    public virtual void SetFlag(string key, TimeSpan expiresIn)
        => SetValue<bool>(key, true, expiresIn);

    /// <summary>
    /// Cache a boolean with the value true for the given duration.
    /// </summary>
    public virtual void SetFlag(string key, DateTime expiresAt)
        => SetValue<bool>(key, true, expiresAt);

    /// <summary>
    /// Check if a boolean with the value true is cached for the given key.
    /// </summary>
    public virtual bool HasFlag(string key)
        => GetValue<bool>(key);

    /// <summary>
    /// Set a cached value of the given type.
    /// </summary>
    public virtual T SetValue<T>(string key, T value, TimeSpan expiresIn, bool allowDistribute = false)
        => SetValue(key, value, DateTime.Now + expiresIn, allowDistribute);

    /// <summary>
    /// Set a cached value of the given type.
    /// </summary>
    public virtual T SetValue<T>(string key, T value, DateTime expiresAt, bool allowDistribute = false)
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

            return value;
        }
    }

    /// <summary>
    /// Clear the given cached entry.
    /// </summary>
    public virtual void ClearKey(string key, bool allowDistribute = false)
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
    public virtual void ClearAll(bool allowDistribute = false)
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
    public virtual void RemoveExpired(TimeSpan? onlyIfLongerThanThisSinceLastCleanup = null)
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
            for (int i = 0; i < keys.Count; i++)
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

    /// <summary>
    /// Adds or updates cache.
    /// </summary>
    public virtual T Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, bool allowDistribute = false)
        => SetValue(key, value, DateTime.Now + absoluteExpirationRelativeToNow, allowDistribute);

    /// <summary>
    /// Attempts to get a cached value.
    /// </summary>
    public virtual bool TryGetValue<T>(string key, out T value)
    {
        value = default;
        if (!ContainsKey(key))
        {
            return false;
        }
        value = GetValue<T>(key);
        return true;
    }

    /// <summary>
    /// Clear the given cached entry.
    /// </summary>
    public virtual void Remove(string key, bool allowDistribute = false) => ClearKey(key, allowDistribute);
}
