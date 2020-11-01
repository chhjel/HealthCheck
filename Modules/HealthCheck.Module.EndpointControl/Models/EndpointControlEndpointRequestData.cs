using System;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Contains details about a request.
    /// </summary>
    public class EndpointControlEndpointRequestData
    {
        /// <summary>
        /// Time of the request. Defaults to now.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Name of the endpoint.
        /// </summary>
        public string EndpointName { get; set; }

        /// <summary>
        /// Unique id of the endpoint.
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// Ip address or something similar to identify the location.
        /// <para>Defaults to first of <c>HTTP_X_FORWARDED_FOR</c>, <c>REMOTE_ADDR</c> and <c>UserHostAddress</c></para>
        /// </summary>
        public string UserLocationIdentifier { get; set; }

        /// <summary>
        /// User-agent string.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Http method of the request.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Target controller type for the request.
        /// </summary>
        public Type ControllerType { get; set; }

        /// <summary>
        /// Target controller for the request.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// Target action for the request.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Full url of the request.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// This property will be updated by the service with the result.
        /// </summary>
        public bool WasBlocked { get; set; }
    }
}
