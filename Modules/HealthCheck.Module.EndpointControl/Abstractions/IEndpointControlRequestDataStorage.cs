using HealthCheck.Module.EndpointControl.Models;

namespace HealthCheck.Module.EndpointControl.Abstractions
{
    /// <summary>
    /// Stores request history.
    /// </summary>
    public interface IEndpointControlRequestHistoryStorage
    {
        /// <summary>
        /// Store data about a given request.
        /// </summary>
        void AddRequest(EndpointControlEndpointRequestData request);
    }
}
