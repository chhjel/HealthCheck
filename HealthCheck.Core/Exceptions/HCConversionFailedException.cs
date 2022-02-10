using System;
using System.Runtime.Serialization;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when a converter fails to convert input.
    /// </summary>
    [Serializable]
    public class HCConversionFailedException : Exception
    {
        /// <summary>
        /// Thrown when a converter fails to convert input.
        /// </summary>
        public HCConversionFailedException() { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected HCConversionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Thrown when a converter fails to convert input.
        /// </summary>
        public HCConversionFailedException(string inputValue, Type missingForType)
            : base($"Could not parse the value '{inputValue}' into a {missingForType.Name}.") { }
    }
}
