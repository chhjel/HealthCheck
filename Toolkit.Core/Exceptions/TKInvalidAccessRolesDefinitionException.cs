using System;
using System.Runtime.Serialization;

namespace QoDL.Toolkit.Core.Exceptions;

/// <summary>
/// Thrown when checking for access rights when the given access roles enum is not defined correctly.
/// </summary>
[Serializable]
public class TKInvalidAccessRolesDefinitionException : Exception
{
    /// <summary>
    /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
    /// </summary>
    public TKInvalidAccessRolesDefinitionException() { }

    /// <summary>
    /// Initializes a new instance of the object with serialized data.
    /// </summary>
    protected TKInvalidAccessRolesDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    /// <summary>
    /// Thrown when checking for access rights when the given access roles enum is not defined correctly.
    /// </summary>
    public TKInvalidAccessRolesDefinitionException(string message)
        : base(message) { }
}
