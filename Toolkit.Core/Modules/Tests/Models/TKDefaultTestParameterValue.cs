using System;

namespace QoDL.Toolkit.Core.Modules.Tests.Models
{
    /// <summary>
    /// Optional default value wrapper that supports caching.
    /// </summary>
    public class TKDefaultTestParameterValue<T> : ITKDefaultTestParameterValue
    {
        /// <summary>
        /// The default value.
        /// </summary>
        public T DefaultValue { get; set; }

        /// <inheritdoc />
        public TimeSpan? CacheDuration { get; set; }

        /// <inheritdoc />
        public object DefaultValueAsObj => DefaultValue;

        /// <summary>
        /// Optional default value wrapper that supports caching.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cacheDuration">Optional duration to cache the default value for in memory.</param>
        public TKDefaultTestParameterValue(T defaultValue, TimeSpan? cacheDuration)
        {
            DefaultValue = defaultValue;
            CacheDuration = cacheDuration;
        }
    }

    /// <summary>
    /// Used internally.
    /// </summary>
    public interface ITKDefaultTestParameterValue
    {
        /// <summary>
        /// Used internally.
        /// </summary>
        object DefaultValueAsObj { get; }

        /// <summary>
        /// Optional duration to cache the default value for in memory.
        /// </summary>
        TimeSpan? CacheDuration { get; }
    }
}
