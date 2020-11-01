using HealthCheck.Module.EndpointControl.Abstractions;

namespace HealthCheck.Module.EndpointControl.Module
{
    /// <summary>
    /// Options for <see cref="HCEndpointControlModule"/>.
    /// </summary>
    public class HCEndpointControlModuleOptions
    {
        /// <summary>
        /// Checks if requests to certain endpoints are allowed to execute.
        /// </summary>
        public IEndpointControlService EndpointControlService { get; set; }
    }
}
