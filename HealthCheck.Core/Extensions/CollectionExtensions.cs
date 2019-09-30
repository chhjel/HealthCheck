using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extension methods related to collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Get a random item from the given list, optionally using the given random instance.
        /// </summary>
        public static T RandomElement<T>(this IEnumerable<T> enumerable, Random random = null)
        {
            if (enumerable.Count() == 0) return default;

            random = random ?? new Random();
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
    }
}
