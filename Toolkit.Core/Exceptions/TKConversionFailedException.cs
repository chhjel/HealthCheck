using System;
using System.Runtime.Serialization;

namespace QoDL.Toolkit.Core.Exceptions;

/// <summary>
/// Thrown when a converter fails to convert input.
/// </summary>
[Serializable]
public class TKConversionFailedException : Exception
{
    /// <summary>
    /// Thrown when a converter fails to convert input.
    /// </summary>
    public TKConversionFailedException() { }

    /// <summary>
    /// Initializes a new instance of the object with serialized data.
    /// </summary>
    protected TKConversionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    /// <summary>
    /// Thrown when a converter fails to convert input.
    /// </summary>
    public TKConversionFailedException(string inputValue, Type missingForType)
        : base($"Could not parse the value '{inputValue}' into a {missingForType.Name}.") { }
}
