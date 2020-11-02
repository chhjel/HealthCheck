using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Utils;

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
        /// <para>Invoked from <see cref="EndpointControlUtils.CountCurrentRequest"/>.</para>
        /// </summary>
        void StoreHistoricalRequestData(EndpointControlEndpointRequestData requestData);
    }
}
