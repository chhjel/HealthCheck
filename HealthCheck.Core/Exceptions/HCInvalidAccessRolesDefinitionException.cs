using System;
using System.Runtime.Serialization;

namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
    /// </summary>
    [Serializable]
    public class HCInvalidAccessRolesDefinitionException : Exception
    {
        /// <summary>
        /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
        /// </summary>
        public HCInvalidAccessRolesDefinitionException() { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected HCInvalidAccessRolesDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
        /// </summary>
        public HCInvalidAccessRolesDefinitionException(string message)
            : base(message) { }
    }
}
