using System;
using System.Runtime.Serialization;

namespace QoDL.Toolkit.WebUI.Exceptions;

/// <summary>
/// Thrown if the FrontEndOptionsViewModel does not validate.
/// </summary>
[Serializable]
public class ConfigValidationException : Exception
{
    /// <summary>
    /// Thrown if the FrontEndOptionsViewModel does not validate.
    /// </summary>
    public ConfigValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the object with serialized data.
    /// </summary>
    protected ConfigValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
