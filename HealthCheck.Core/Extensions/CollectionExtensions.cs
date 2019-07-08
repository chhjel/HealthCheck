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
    }
}
