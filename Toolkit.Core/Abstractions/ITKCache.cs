using System;

namespace QoDL.Toolkit.Core.Abstractions;

/// <summary>
/// Caches things.
/// </summary>
public interface ITKCache
{
    /// <summary>
    /// Add or replace a cached value.
    /// </summary>
    TItem Set<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow, bool allowDistribute = false);

    /// <summary>
    /// Try to get a cached item.
    /// </summary>
    bool TryGetValue<TItem>(string key, out TItem value);

    /// <summary>
    /// Remove a cached item.
    /// </summary>
    void Remove(string key, bool allowDistribute = false);
}
