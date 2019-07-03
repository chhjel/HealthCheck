using System;

namespace HealthCheck.Web.Core.Exceptions
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
