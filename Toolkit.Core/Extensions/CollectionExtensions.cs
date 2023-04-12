using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QoDL.Toolkit.Core.Extensions;

/// <summary>
/// Extension methods related to collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Adds the values to the given existing average values, handling arithmetic overflows.
    /// </summary>
    public static long AddToAverageWithOverflowProtection(this IEnumerable<long> newValues, long currentAverage, long currentCount)
    {
        double newValueCount = newValues.Count();
        double a = (((double)currentAverage * (double)currentCount) + ((double)newValues.AverageWithOverflowProtection() * newValueCount));
        double b = ((double)currentCount + newValueCount);
        var avg = Math.Round(a / b);

        if (avg >= long.MaxValue) return long.MaxValue;
        else if (avg <= long.MinValue) return long.MinValue;
        else return (long)avg;
    }

    /// <summary>
    /// Averages the given long values, handling arithmetic overflows.
    /// </summary>
    public static long AverageWithOverflowProtection(this IEnumerable<long> values)
    {
        if (values?.Any() != true) return 0;
        var avg = values.Select(x => (double)x).Average();
        avg = Math.Round(avg);

        if (avg >= long.MaxValue) return long.MaxValue;
        else if (avg <= long.MinValue) return long.MinValue;
        else return (long)avg;
    }

    /// <summary>
    /// Create a dictionary, keeping the last of any duplicates.
    /// </summary>
    public static Dictionary<TKey, TElement> ToDictionaryIgnoreDuplicates<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
        if (source?.Any() != true)
        {
            return new();
        }

        var dict = new Dictionary<TKey, TElement>();
        foreach (var item in source)
        {
            dict[keySelector(item)] = elementSelector(item);
        }
        return dict;
    }

    /// <summary>
    /// Create a dictionary, keeping the last of any duplicates.
    /// </summary>
    public static Dictionary<TKey, TSource> ToDictionaryIgnoreDuplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        if (source?.Any() != true)
        {
            return new();
        }

        var dict = new Dictionary<TKey, TSource>();
        foreach (var item in source)
        {
            dict[keySelector(item)] = item;
        }
        return dict;
    }

    /// <summary>
    /// Take the last n items up to the given max.
    /// </summary>
    public static IEnumerable<T> TakeLastN<T>(this IEnumerable<T> enumerable, int max)
        => enumerable.Skip(Math.Max(0, enumerable.Count() - max));

    /// <summary>
    /// Get a random item from the given list, optionally using the given random instance.
    /// </summary>
    public static T RandomElement<T>(this IEnumerable<T> enumerable, Random random = null)
    {
        if (!enumerable.Any()) return default;

        random ??= new Random();
        int index = random.Next(0, enumerable.Count());
        return enumerable.ElementAt(index);
    }

    /// <summary>
    /// Order by descending or ascending dependent on the given bool.
    /// </summary>
    public static IOrderedEnumerable<TSource> OrderByWithDirection<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool descending)
        => descending
            ? source.OrderByDescending(keySelector)
            : source.OrderBy(keySelector);

    /// <summary>
    /// Throws a <see cref="OperationCanceledException"/> if the given token is cancelled.
    /// </summary>
    public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> en, CancellationToken token)
    {
        foreach (var item in en)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}
