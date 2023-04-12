using System;
using System.Runtime.Serialization;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// Thrown when a test method has a wrong signature.
/// </summary>
[Serializable]
public class InvalidTestDefinitionException : Exception
{
    /// <summary>
    /// Create a new <see cref="InvalidTestDefinitionException"/>.
    /// </summary>
    public InvalidTestDefinitionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the object with serialized data.
    /// </summary>
    protected InvalidTestDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
