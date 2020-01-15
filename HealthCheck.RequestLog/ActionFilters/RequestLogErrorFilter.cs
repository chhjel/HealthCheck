#if NETFULL
using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.RequestLog.Enums;
using HealthCheck.Core.Modules.RequestLog.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System;

namespace HealthCheck.RequestLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs unhandled MVC exceptions.
    /// </summary>
    public class RequestLogErrorFilter : HandleErrorAttribute
    {
        private IRequestLogService RequestLogService => RequestLogServiceAccessor.Current;

        /// <summary>
        /// Intercepts and logs unhandled MVC exceptions.
        /// </summary>
        public override void OnException(ExceptionContext context)
        {
            var ip = RequestUtils.GetIPAddress(context?.HttpContext?.Request);
            Task.Run(() => SendEventToService(context, ip));
            base.OnException(context);
        }

        private void SendEventToService(ExceptionContext context, string ip)
        {
            try
            {
                var routeData = context.RouteData;
                var controllerType = context.Controller?.GetType();
                var controllerName = routeData.Values["controller"] as string;
                var actionName = routeData.Values["action"] as string;
                var actionMethod = context.Controller?.ViewBag?.RequestLog_ActionMethodInfo;
                var requestMethod = context?.RequestContext?.HttpContext?.Request?.HttpMethod;
                var url = RequestUtils.GetUrl(context.HttpContext?.Request);
                int statusCode = (context.Exception != null)
                    ? new HttpException(null, context.Exception).GetHttpCode()
                    : context.HttpContext?.Response?.StatusCode ?? 500;

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
                    Exception = context.Exception,
                    Result = context.Result?.ToString(),
                    SourceIP = ip
                });
            }
            catch(Exception) {}
        }
    }
}
#endif
