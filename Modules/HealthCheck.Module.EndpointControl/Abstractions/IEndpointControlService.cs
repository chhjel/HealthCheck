using HealthCheck.Module.EndpointControl.Models;

namespace HealthCheck.Module.EndpointControl.Abstractions
{
    /// <summary>
    /// Checks if requests to certain endpoints are allowed to execute.
    /// </summary>
    public interface IEndpointControlService
    {
        /// <summary>
        /// Tracks the given request data and returns true if it is allowed to go through.
        /// </summary>
        bool HandleRequest(EndpointControlEndpointRequestData requestData, bool storeData);

        /// <summary>
        /// Use to manually store request data.
        /// <para>Invoked from <c>EndpointControlUtils.CountCurrentRequest</c>.</para>
        /// </summary>
        void StoreHistoricalRequestData(EndpointControlEndpointRequestData requestData);
    }
}
