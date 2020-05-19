using HealthCheck.Core.Abstractions;
using System;

namespace HealthCheck.Core.Serializers
{
    /// <summary>
    /// Only returns an empty string.
    /// </summary>
    public class DumpNullJsonSerializer : IJsonSerializer
    {
        /// <summary>
        /// Returns null.
        /// </summary>
        public object Deserialize(string json, Type type) => null;

        /// <summary>
        /// Returns default.
        /// </summary>
        public T Deserialize<T>(string json) => default;

        /// <summary>
        /// Returns an empty string.
        /// </summary>
        public string Serialize(object obj) => "";
    }
}
