#if NETFULL
using HealthCheck.Core.Config;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Models;
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
        internal const string RequestItemKey_RequestData = "HC_HCControlledEndpoint_RequestData";

#if NETFULL
        private static readonly object _countLock = new object();

        /// <summary>
        /// Invoke manually from an action when using <see cref="HCControlledEndpointAttribute.ManuallyCounted"/> to store the request data where it's suitable.
        /// <para>More than one call to this method per request is ignored, so call it as many times you want.</para>
        /// </summary>
        public static void CountCurrentRequest()
        {
            EndpointControlEndpointRequestData requestData = null;

            lock (_countLock)
            {
                // Get data stored in attribute
                requestData = HttpContext.Current?.Items?.Contains(RequestItemKey_RequestData) != true
                    ? null
                    : HttpContext.Current?.Items?[RequestItemKey_RequestData] as EndpointControlEndpointRequestData;

                if (requestData == null)
                {
                    return;
                }

                // Don't allow multiple calls to this per request by removing the data at once
                HttpContext.Current.Items.Remove(RequestItemKey_RequestData);
            }

            // Send it to the service
            var service = HCGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlService)) as IEndpointControlService;
            service?.StoreHistoricalRequestData(requestData);
        }

        /// <summary>
        /// This method returns true if <see cref="IEndpointControlService.HandleRequest"/> returned false for this request.
        /// </summary>
        public static bool CurrentRequestWasDecidedBlocked()
        {
            return HttpContext.Current?.Items?.Contains(RequestItemKey_Allowed) == true
                && !(bool)HttpContext.Current?.Items?[RequestItemKey_Allowed];
        }
#endif
    }
}
