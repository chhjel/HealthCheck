using System;
using System.Runtime.Serialization;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
    /// </summary>
    [Serializable]
    public class InvalidAccessRolesDefinitionException : System.Exception
    {
        /// <summary>
        /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
        /// </summary>
        public InvalidAccessRolesDefinitionException() { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected InvalidAccessRolesDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
        /// </summary>
        public InvalidAccessRolesDefinitionException(string message)
            : base(message) { }
    }
}
