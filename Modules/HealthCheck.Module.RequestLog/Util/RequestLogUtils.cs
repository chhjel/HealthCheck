#if NETFULL
using HealthCheck.RequestLog.Abstractions;
using HealthCheck.RequestLog.Enums;
using HealthCheck.RequestLog.Models;
using System;
using System.Web;

namespace HealthCheck.Module.RequestLog.Util
{
    /// <summary>
    /// Utility methods for <see cref="IRequestLogService"/>.
    /// </summary>
    public static class RequestLogUtils
    {
        /// <summary>
        /// Utility methods for <see cref="IRequestLogService"/>.
        /// </summary>
        public static void HandleRequest(IRequestLogService service, Type controllerType, HttpRequest request,
            string action = "PageLoad", string controllerName = null, string ip = null, string forcedControllerType = null)
        {
            try
            {
                var requestMethod = request?.HttpMethod;
                var url = RequestUtils.GetUrl(request);

                service.HandleRequestEvent(new LogFilterEvent()
                {
                    FilterMethod = LogFilterMethod.OnResultExecuted,
                    ControllerType = controllerType,
                    Controller = controllerName ?? controllerType.Name,
                    Action = action,
                    ActionMethod = null,
                    RequestMethod = requestMethod,
                    Url = url,
                    Result = "",
                    StatusCode = null,
                    SourceIP = ip ?? RequestUtils.GetIPAddress(request?.RequestContext?.HttpContext?.Request),
                    ForcedControllerType = forcedControllerType
                });
            }
            catch (Exception) { /* Ignore error here */ }
        }
    }
}
#endif
