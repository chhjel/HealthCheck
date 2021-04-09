using HealthCheck.Core.Models;
using System;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Serializes and deserializes data.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Serialize the given object into json.
        /// <para>
        /// Should ignore errors and indent result json by default.
        /// </para>
        /// </summary>
        string Serialize(object obj, bool pretty = true);

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        object Deserialize(string json, Type type);

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        T Deserialize<T>(string json);

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        HCGenericResult<object> DeserializeExt(string json, Type type);
    }
}
