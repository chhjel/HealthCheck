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
        /// Is set automatically to the full url of the request.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Is set automatically to all header keys and values.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// POST, GET etc
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// When the request started.
        /// </summary>
        public DateTime RequestExecutionStartTime { get; internal set; }
    }
}
