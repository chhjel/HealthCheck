namespace QoDL.Toolkit.Core.Extensions;

/// <summary>
/// Extensions for numeric types.
/// </summary>
public static class TKNumberExtensions
{
    /// <summary>
    /// Clamp the given value between the given inclusive values.
    /// </summary>
    public static int Clamp(this int value, int min, int max)
    {
        if (value <= min) return min;
        else if (value >= max) return max;
        else return value;
    }
}
