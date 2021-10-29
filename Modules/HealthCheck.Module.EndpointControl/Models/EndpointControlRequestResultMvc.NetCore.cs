#if NETCORE
using HealthCheck.Module.EndpointControl.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// MVC result from <see cref="IEndpointControlRequestResult"/>.
    /// </summary>
    public class EndpointControlRequestResultMvc : EndpointControlRequestResultBase
    {
        /// <summary>
        /// Result to return if any.
        /// </summary>
        public IActionResult Result { get; set; }

        /// <summary>
        /// MVC result from <see cref="IEndpointControlRequestResult"/>.
        /// </summary>
        public EndpointControlRequestResultMvc(IActionResult result = null)
        {
            Result = result;
        }
    }
}
#endif
