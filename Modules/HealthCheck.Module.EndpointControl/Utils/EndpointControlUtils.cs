#if NETFULL
using HealthCheck.Module.EndpointControl.Abstractions;
using System.Web;
#endif

namespace HealthCheck.Module.EndpointControl.Utils
{
    /// <summary>
    /// Utilities related to the Endpoint Control healthcheck module.
    /// </summary>
    public static class EndpointControlUtils
    {
        internal const string RequestItemKey_Allowed = "HC_HCControlledEndpoint_Allowed";

#if NETFULL
        /// <summary>
        /// This method returns true if <see cref="IEndpointControlService.HandleRequest(Models.EndpointControlEndpointRequestData)"/> returned false for this request.
        /// </summary>
        public static bool CurrentRequestWasDecidedBlocked()
        {
            return HttpContext.Current?.Items?.Contains(RequestItemKey_Allowed) == true
                && !(bool)HttpContext.Current?.Items?[RequestItemKey_Allowed];
        }
#endif
    }
}
