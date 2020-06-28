using HealthCheck.Core.Extensions;
using System;
using System.Runtime.Serialization;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when a converter is not registered for the type we want to convert to.
    /// </summary>
    [Serializable]
    public class ConversionHandlerNotFoundException : Exception
    {
        /// <summary>
        /// Thrown when a converter is not registered for the type we want to convert to.
        /// </summary>
        public ConversionHandlerNotFoundException() { }

        /// <summary>
        /// Thrown when a converter is not registered for the type we want to convert to.
        /// </summary>
        public ConversionHandlerNotFoundException(string inputValue, Type missingForType)
            : this($"No handler for the type '{missingForType.GetFriendlyTypeName()}' is registered, or could not parse the value '{inputValue}' into a {missingForType.GetFriendlyTypeName()}.") { }

        /// <summary>
        /// Thrown when a converter is not registered for the type we want to convert to.
        /// </summary>
        public ConversionHandlerNotFoundException(string message)
            : base(message) { }

        /// <summary>
        /// Thrown when a converter is not registered for the type we want to convert to.
        /// </summary>
        public ConversionHandlerNotFoundException(string message, System.Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected ConversionHandlerNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
