using HealthCheck.Core.Util;

namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extension methods related to <see cref="Maybe{T}"/>.
    /// </summary>
    public static class MaybeExtensions
    {
        /// <summary>
        /// Check if either the maybe is null, the maybe has no value set, or the value of the maybe is null.
        /// </summary>
        public static bool HasNothing<T>(this Maybe<T> maybe) 
            => maybe == null || !maybe.HasValue || maybe.Value == null;

        /// <summary>
        /// Check that the maybe is not null and contains a value that is not null.
        /// </summary>
        public static bool HasValue<T>(this Maybe<T> maybe)
            => maybe != null && maybe.HasValue && maybe.Value != null;
    }
}
