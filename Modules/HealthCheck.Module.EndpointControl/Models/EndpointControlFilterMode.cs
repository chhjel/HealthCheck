namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Filter mode.
    /// </summary>
    public enum EndpointControlFilterMode
    {
        /// <summary>
        /// Match full value.
        /// </summary>
        Matches = 0,

        /// <summary>
        /// Match partial value.
        /// </summary>
        Contains,

        /// <summary>
        /// RegEx match.
        /// </summary>
        RegEx
    }
}
