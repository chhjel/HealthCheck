using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Models
{
    /// <summary>
    /// Shared request model for .NET Framework and .NET Core.
    /// </summary>
    public class HCRequestContext
    {
        /// <summary>
        /// True as long as we're in a request context. When e.g. created from a scheduled job this will most likely be false.
        /// </summary>
        public bool HasRequestContext { get; set; }

        /// <summary>
        /// Is set automatically to the full url of the request.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Is set automatically to all header keys and values.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Is set automatically to all request cookies.
        /// </summary>
        public Dictionary<string, string> Cookies { get; set; }

        /// <summary>
        /// POST, GET etc
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// When the request started.
        /// </summary>
        public DateTimeOffset RequestExecutionStartTime { get; internal set; }
    }
}
