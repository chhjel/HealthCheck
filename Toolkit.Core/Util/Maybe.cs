namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Nullable reference type before it was implemented.
/// </summary>
public class Maybe<T>
{
    /// <summary>
    /// True if constructor was called with a value.
    /// </summary>
    public bool HasValue { get; private set; }

    /// <summary>
    /// The value if any.
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Without value.
    /// </summary>
    public Maybe() { }

    /// <summary>
    /// With value.
    /// </summary>
    public Maybe(T value)
    {
        HasValue = true;
        Value = value;
    }
}
