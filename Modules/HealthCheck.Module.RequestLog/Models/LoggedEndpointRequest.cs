using System;

namespace HealthCheck.RequestLog.Models
{
    /// <summary>
    /// A logged request.
    /// </summary>
    public class LoggedEndpointRequest
    {
        /// <summary>
        /// Time the request was logged.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Application version the request was logged in.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Status code the request returned if any.
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Exception if any.
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Source IP address.
        /// </summary>
        public string SourceIP { get; set; }

        /// <summary>
        /// Url that was requested.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// True if there was no exception.
        /// </summary>
        public bool IsSuccess => (ErrorDetails == null);

        /// <summary>
        /// True if there was any exception.
        /// </summary>
        public bool IsError => !IsSuccess;
    }
}
