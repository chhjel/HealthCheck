using HealthCheck.Core.Models;
using System.Collections.Generic;
using System.IO;

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
        /// GET, POST etc.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// True if the request is a POST request.
        /// </summary>
        public bool IsPOST => Method?.ToLower()?.Trim() == "post";

        /// <summary>
        /// True if the request is a GET request.
        /// </summary>
        public bool IsGET => Method?.ToLower()?.Trim() == "get";

        /// <summary>
        /// The clients user agent if any.
        /// </summary>
        public string UserAgent => Headers?.ContainsKey("User-Agent") == true ? Headers["User-Agent"] : null;

        /// <summary>
        /// The clients IP address if any.
        /// </summary>
        public string ClientIP { get; set; }

        internal Stream InputStream { get; set; }
        internal List<RequestFormFile> FormFiles { get; set; }
    }
}
