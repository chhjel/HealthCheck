using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util.Collections;

internal class ListWithExpiration<T>
{
    public int MaxCount { get; set; } = 10000;

    private readonly List<ListWithExpirationItem> _items = new();

    private struct ListWithExpirationItem
    {
        public DateTime ExpiresAt;
        public T Value;
    }

    public bool Any(Func<T, bool> condition)
    {
        lock(_items)
        {
            return _items
                .Where(x => !IsExpired(x))
                .Select(x => x.Value)
                .Any(condition);
        }
    }

    public void RemoveWhere(Func<T, bool> condition)
    {
        lock (_items)
        {
            _items.RemoveAll(x => condition(x.Value));
        }
    }

    public void Add(T value, DateTime expiresAt)
    {
        lock (_items)
        {
            if (_items.Count >= MaxCount)
            {
                _items.RemoveAt(0);
            }

            _items.Add(new ListWithExpirationItem
            {
                Value = value,
                ExpiresAt = expiresAt
            });
        }
    }

    public void ClearAll()
    {
        lock (_items)
        {
            _items.Clear();
        }
    }

    public void RemoveExpired()
    {
        lock(_items)
        {
            var toRemove = new List<ListWithExpirationItem>();
            foreach (var item in _items)
            {
                if (IsExpired(item))
                {
                    toRemove.Add(item);
                }
            }

            foreach(var item in toRemove)
            {
                _items.Remove(item);
            }
        }
    }

    private bool IsExpired(ListWithExpirationItem item) => item.ExpiresAt <= DateTime.Now;
}
