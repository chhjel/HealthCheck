using System;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when a converter fails to convert input.
    /// </summary>
    [Serializable]
    public class ConversionFailedException : System.Exception
    {
        /// <summary>
        /// Thrown when a converter fails to convert input.
        /// </summary>
        public ConversionFailedException() { }

        /// <summary>
        /// Thrown when a converter fails to convert input.
        /// </summary>
        public ConversionFailedException(string inputValue, Type missingForType)
            : base($"Could not parse the value '{inputValue}' into a {missingForType.Name}.") { }
    }
}
