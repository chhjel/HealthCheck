#if NETFULL
using QoDL.Toolkit.Module.RequestLog.Util;
using QoDL.Toolkit.RequestLog.Abstractions;
using QoDL.Toolkit.RequestLog.Enums;
using QoDL.Toolkit.RequestLog.Models;
using QoDL.Toolkit.RequestLog.Services;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QoDL.Toolkit.RequestLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs unhandled MVC exceptions.
    /// </summary>
    public class RequestLogErrorFilterAttribute : HandleErrorAttribute
    {
        private IRequestLogService RequestLogService => RequestLogServiceAccessor.Current;

        /// <summary>
        /// Intercepts and logs unhandled MVC exceptions.
        /// </summary>
        public override void OnException(ExceptionContext filterContext)
        {
            var ip = RequestUtils.GetIPAddress(filterContext?.HttpContext?.Request);
            Task.Run(() => SendEventToService(filterContext, ip));
            base.OnException(filterContext);
        }

        private void SendEventToService(ExceptionContext filterContext, string ip)
        {
            try
            {
                var routeData = filterContext.RouteData;
                var controllerType = filterContext.Controller?.GetType();
                var controllerName = routeData.Values["controller"] as string;
                var actionName = routeData.Values["action"] as string;
                var actionMethod = filterContext.Controller?.ViewBag?.RequestLog_ActionMethodInfo;
                var requestMethod = filterContext?.RequestContext?.HttpContext?.Request?.HttpMethod;
                var url = RequestUtils.GetUrl(filterContext.HttpContext?.Request);
                int statusCode = (filterContext.Exception != null)
                    ? new HttpException(null, filterContext.Exception).GetHttpCode()
                    : filterContext.HttpContext?.Response?.StatusCode ?? 500;

                RequestLogService.HandleRequestEvent(new LogFilterEvent()
                {
                    FilterMethod = LogFilterMethod.OnException,
                    ControllerType = controllerType,
                    Controller = controllerName,
                    Action = actionName,
                    ActionMethod = actionMethod,
                    RequestMethod = requestMethod,
                    Url = url,
                    StatusCode = statusCode.ToString(),
                    Exception = filterContext.Exception,
                    Result = filterContext.Result?.ToString(),
                    SourceIP = ip
                });
            }
            catch(Exception) { /* Ignore errors here */ }
        }
    }
}
#endif
