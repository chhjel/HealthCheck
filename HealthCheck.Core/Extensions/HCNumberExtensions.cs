namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extensions for numeric types.
    /// </summary>
    public static class HCNumberExtensions
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
}
