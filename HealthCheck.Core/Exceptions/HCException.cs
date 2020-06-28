using System;
using System.Runtime.Serialization;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Generic exception thrown from HealthCheck code when no other more specific exception is implemented yet.
    /// </summary>
    [Serializable]
    public class HCException : Exception
    {
        /// <summary>
        /// Generic exception thrown from HealthCheck code.
        /// </summary>
        public HCException() { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected HCException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Generic exception thrown from HealthCheck code.
        /// </summary>
        public HCException(string message) : base(message) { }
    }
}
