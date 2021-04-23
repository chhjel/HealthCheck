using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Models;
using System;

namespace HealthCheck.Core.Serializers
{
    /// <summary>
    /// Returns empty strings and nulls.
    /// </summary>
    public class DumpNullJsonSerializer : IJsonSerializer
    {
        /// <inheritdoc />
        public string LastError { get; set; }

        /// <summary>
        /// Returns null.
        /// </summary>
        public object Deserialize(string json, Type type) => null;

        /// <summary>
        /// Returns default.
        /// </summary>
        public T Deserialize<T>(string json) => default;

        /// <summary>
        /// Returns success w/ default.
        /// </summary>
        public HCGenericResult<object> DeserializeExt(string json, Type type) => HCGenericResult<object>.CreateSuccess(default);

        /// <summary>
        /// Returns an empty string.
        /// </summary>
        public string Serialize(object obj, bool pretty = true) => "";
    }
}
