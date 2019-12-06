using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.RequestLog.Models
{
    /// <summary>
    /// A group of requests/errors for a single endpoint.
    /// </summary>
    public class LoggedEndpointDefinition
    {
        /// <summary>
        /// Entry id.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Id of the endpoint.
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// Name of the endpoint.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the endpoint.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Group this endpoint belongs to.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 'MVC' or 'WebApi'
        /// </summary>
        public string ControllerType { get; set; }

        /// <summary>
        /// Controller name.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// HTTP verb for the endpoint.
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// Typename of the controller.
        /// </summary>
        public string FullControllerName { get; set; }

        /// <summary>
        /// Action name.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Url to the endpoint without any query string.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Any successfull calls.
        /// </summary>
        public List<LoggedEndpointRequest> Calls { get; set; } = new List<LoggedEndpointRequest>();

        /// <summary>
        /// Any unsuccessfull calls.
        /// </summary>
        public List<LoggedEndpointRequest> Errors { get; set; } = new List<LoggedEndpointRequest>();
    }
}
