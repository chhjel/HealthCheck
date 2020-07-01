using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Some data regarding the current request.
    /// </summary>
    public class HealthCheckModuleRequestData
    {
        /// <summary>
        /// Full relative url. Everything after the domain.
        /// </summary>
        public string RelativeUrl { get; set; }

        /// <summary>
        /// Header values.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The clients user agent if any.
        /// </summary>
        public string UserAgent => Headers?.ContainsKey("User-Agent") == true ? Headers["User-Agent"] : null;

        /// <summary>
        /// The clients IP address if any.
        /// </summary>
        public string ClientIP { get; set; }
    }
}
