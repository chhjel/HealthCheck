#if NETFULL
using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.RequestLog.Enums;
using HealthCheck.Core.Modules.RequestLog.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System;

namespace HealthCheck.RequestLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs successfull WebApi requests and unhandled exceptions.
    /// </summary>
    public class RequestLogWebApiActionFilter : ActionFilterAttribute
    {
        private IRequestLogService RequestLogService => RequestLogServiceAccessor.Current;

        /// <summary>
        /// Intercepts and logs successfull WebApi requests and unhandled exceptions.
        /// </summary>
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            var ip = RequestUtils.GetIPAddress(context?.Request);
            if (ip == "::1")
            {
                ip = "localhost";
            }
            Task.Run(() => SendActionEventToService(RequestLogService, LogFilterMethod.OnActionExecuted, context, ip));
            base.OnActionExecuted(context);
        }

        internal static void SendActionEventToService(IRequestLogService service, LogFilterMethod method, HttpActionExecutedContext context, string ip)
        {
            try
            {
                var controllerType = context.ActionContext.ControllerContext.Controller?.GetType();
                var controllerName = context.ActionContext.ControllerContext?.ControllerDescriptor?.ControllerName;
                var actionName = context.ActionContext?.ActionDescriptor?.ActionName;
                var actionMethod = (context.ActionContext?.ActionDescriptor is ReflectedHttpActionDescriptor reflectedHttpActionDescriptor)
                    ? reflectedHttpActionDescriptor.MethodInfo : null;
                var requestMethod = context.Request.Method?.ToString();
                var url = context.ActionContext.Request.RequestUri?.ToString();
                string result = context.Exception?.ToString();
                int statusCode = (context.Response?.StatusCode != null)
                    ? (int)context.Response.StatusCode
                    : (context.Exception != null) ? new HttpException(null, context.Exception).GetHttpCode() : 0;

                service.HandleRequestEvent(new LogFilterEvent()
                {
                    FilterMethod = method,
                    ControllerType = controllerType,
                    Controller = controllerName,
                    Action = actionName,
                    ActionMethod = actionMethod,
                    RequestMethod = requestMethod,
                    Url = url,
                    Result = result,
                    StatusCode = statusCode.ToString(),
                    Exception = context.Exception,
                    SourceIP = ip
                });
            }
            catch (Exception) {}
        }
    }
}
#endif
