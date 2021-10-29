using HealthCheck.Core.Config;
#if NETFULL
using System.Web;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Models;
#endif
#if NETCORE
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Models;
using Microsoft.AspNetCore.Http;
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

        private static readonly object _countLock = new object();

#if NETFULL
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
                HttpContext.Current?.Items?.Remove(RequestItemKey_RequestData);
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

#if NETCORE
        internal static HttpContext GetCurrentRequest()
        {
            var accessor = HCGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            return accessor?.HttpContext;
        }

        /// <summary>
        /// Invoke manually from an action when using <see cref="HCControlledEndpointAttribute.ManuallyCounted"/> to store the request data where it's suitable.
        /// <para>More than one call to this method per request is ignored, so call it as many times you want.</para>
        /// </summary>
        public static void CountCurrentRequest()
        {
            EndpointControlEndpointRequestData requestData = null;

            lock (_countLock)
            {
                var request = GetCurrentRequest();

                // Get data stored in attribute
                requestData = request?.Items?.ContainsKey(RequestItemKey_RequestData) != true
                    ? null
                    : request?.Items?[RequestItemKey_RequestData] as EndpointControlEndpointRequestData;

                if (requestData == null)
                {
                    return;
                }

                // Don't allow multiple calls to this per request by removing the data at once
                request?.Items?.Remove(RequestItemKey_RequestData);
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
            var request = GetCurrentRequest();
            return request?.Items?.ContainsKey(RequestItemKey_Allowed) == true
                && !(bool)request?.Items?[RequestItemKey_Allowed];
        }
#endif
    }
}
