using System;
using System.Runtime.Serialization;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when a converter fails to convert input.
    /// </summary>
    [Serializable]
    public class ConversionFailedException : Exception
    {
        /// <summary>
        /// Thrown when a converter fails to convert input.
        /// </summary>
        public ConversionFailedException() { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected ConversionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Thrown when a converter fails to convert input.
        /// </summary>
        public ConversionFailedException(string inputValue, Type missingForType)
            : base($"Could not parse the value '{inputValue}' into a {missingForType.Name}.") { }
    }
}
