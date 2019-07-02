using System;

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
        /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
        /// </summary>
        public InvalidAccessRolesDefinitionException(string message)
            : base(message) { }
    }
}
