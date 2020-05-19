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
        /// Should ignore errors and indent result json.
        /// </para>
        /// </summary>
        string Serialize(object obj);

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        object Deserialize(string json, Type type);

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        T Deserialize<T>(string json);
    }
}
