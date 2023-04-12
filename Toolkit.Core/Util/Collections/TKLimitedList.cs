using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util.Collections;

/// <summary>
/// List with max limit.
/// <para>All adds and inserts will remove the oldest item if max limit is broken.</para>
/// </summary>
public class TKLimitedList<T> : List<T>
{
    /// <summary>
    /// Items at start of list will be removed to keep the count at this limit.
    /// </summary>
    public int MaxItemLimit { get; set; }

    /// <summary>
    /// List with max limit.
    /// <para>All adds and inserts will remove the oldest item if max limit is broken.</para>
    /// </summary>
    /// <param name="maxItemLimit">Items at start of list will be removed to keep the count at this limit.</param>
    public TKLimitedList(int maxItemLimit)
    {
        MaxItemLimit = maxItemLimit;
    }

    /// <summary>
    /// Adds an object to the end of the list.
    /// </summary>
    public new void Add(T item)
    {
        CheckLimit(1);
        base.Add(item);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the list.
    /// </summary>
    public new void AddRange(IEnumerable<T> collection)
    {
        var additionCount = collection.Count();
        if (additionCount > MaxItemLimit)
        {
            var skipCount = additionCount - MaxItemLimit;
            collection = collection.Skip(skipCount);
            additionCount = MaxItemLimit;
        }

        CheckLimit(additionCount);
        base.AddRange(collection);
    }

    /// <summary>
    /// Inserts the given element at the given position.
    /// </summary>
    public new void Insert(int index, T item)
    {
        CheckLimit(1);
        base.Insert(index, item);
    }

    /// <summary>
    /// Inserts the given elements at the given position.
    /// </summary>
    public new void InsertRange(int index, IEnumerable<T> collection)
    {
        var additionCount = collection.Count();
        if (additionCount > MaxItemLimit)
        {
            var skipCount = additionCount - MaxItemLimit;
            collection = collection.Skip(skipCount);
            additionCount = MaxItemLimit;
        }

        CheckLimit(additionCount);
        base.InsertRange(index, collection);
    }

    private void CheckLimit(int additionCount)
    {
        var countAfterAdded = Count + additionCount;
        if (countAfterAdded > MaxItemLimit)
        {
            var toRemove = countAfterAdded - MaxItemLimit;
            RemoveRange(0, toRemove);
        }
    }
}
