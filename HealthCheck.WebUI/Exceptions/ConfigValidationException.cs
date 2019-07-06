using System;

namespace HealthCheck.WebUI.Exceptions
{
    /// <summary>
    /// Thrown if the FrontEndOptionsViewModel does not validate.
    /// </summary>
    public class ConfigValidationException : Exception
    {
        /// <summary>
        /// Thrown if the FrontEndOptionsViewModel does not validate.
        /// </summary>
        public ConfigValidationException(string message) : base(message) {}
    }
}
