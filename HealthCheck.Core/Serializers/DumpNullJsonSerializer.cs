using HealthCheck.Core.Abstractions;

namespace HealthCheck.Core.Serializers
{
    /// <summary>
    /// Only returns an empty string.
    /// </summary>
    public class DumpNullJsonSerializer : IDumpJsonSerializer
    {
        /// <summary>
        /// Only returns an empty string.
        /// </summary>
        public string Serialize(object obj) => "";
    }
}
