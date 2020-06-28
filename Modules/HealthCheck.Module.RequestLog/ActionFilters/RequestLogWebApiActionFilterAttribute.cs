#if NETFULL
using HealthCheck.RequestLog.Abstractions;
using HealthCheck.RequestLog.Enums;
using HealthCheck.RequestLog.Models;
using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace HealthCheck.RequestLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs successfull WebApi requests and unhandled exceptions.
    /// </summary>
    public class RequestLogWebApiActionFilterAttribute : ActionFilterAttribute
    {
        private IRequestLogService RequestLogService => RequestLogServiceAccessor.Current;

        /// <summary>
        /// Intercepts and logs successfull WebApi requests and unhandled exceptions.
        /// </summary>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var ip = RequestUtils.GetIPAddress(actionExecutedContext?.Request);
            if (ip == "::1")
            {
                ip = "localhost";
            }
            Task.Run(() => SendActionEventToService(RequestLogService, LogFilterMethod.OnActionExecuted, actionExecutedContext, ip));
            base.OnActionExecuted(actionExecutedContext);
        }

        internal static void SendActionEventToService(IRequestLogService service, LogFilterMethod method, HttpActionExecutedContext actionExecutedContext, string ip)
        {
            try
            {
                var controllerType = actionExecutedContext.ActionContext.ControllerContext.Controller?.GetType();
                var controllerName = actionExecutedContext.ActionContext.ControllerContext?.ControllerDescriptor?.ControllerName;
                var actionName = actionExecutedContext.ActionContext?.ActionDescriptor?.ActionName;
                var actionMethod = (actionExecutedContext.ActionContext?.ActionDescriptor is ReflectedHttpActionDescriptor reflectedHttpActionDescriptor)
                    ? reflectedHttpActionDescriptor.MethodInfo : null;
                var requestMethod = actionExecutedContext.Request.Method?.ToString();
                var url = RequestUtils.GetUrl(actionExecutedContext.ActionContext.Request);
                string result = actionExecutedContext.Exception?.ToString();

                int statusCode = 0;
                if (actionExecutedContext.Response?.StatusCode != null)
                {
                    statusCode = (int)actionExecutedContext.Response.StatusCode;
                }
                else if (actionExecutedContext.Exception != null)
                {
                    statusCode = new HttpException(null, actionExecutedContext.Exception).GetHttpCode();
                }

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
                    Exception = actionExecutedContext.Exception,
                    SourceIP = ip
                });
            }
            catch (Exception) { /* Ignore errors here */ }
        }
    }
}
#endif
