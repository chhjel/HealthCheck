#if NETFULL
using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.RequestLog.Enums;
using HealthCheck.Core.Modules.RequestLog.Models;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace HealthCheck.RequestLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs successfull MVC requests.
    /// </summary>
    public class RequestLogActionFilter : ActionFilterAttribute
    {
        private IRequestLogService RequestLogService => RequestLogServiceAccessor.Current;

        /// <summary>
        /// Intercepts and logs successfull MVC requests.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // TaskAsyncActionDescriptor
            if (context.Controller?.ViewBag != null)
            {
                MethodInfo actionMethod = null;
                if (context.ActionDescriptor is ReflectedActionDescriptor reflectedActionDescriptor)
                {
                    actionMethod = reflectedActionDescriptor?.MethodInfo;
                }
                else if (context.ActionDescriptor is TaskAsyncActionDescriptor taskAsyncActionDescriptor)
                {
                    actionMethod = taskAsyncActionDescriptor?.TaskMethodInfo;
                }
                context.Controller.ViewBag.RequestLog_ActionMethodInfo = actionMethod;
                context.Controller.ViewBag.RequestLog_SourceIP = RequestUtils.GetIPAddress(context?.HttpContext?.Request);
            }
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Intercepts and logs successfull MVC requests.
        /// </summary>
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            Task.Run(() => SendEventToService(LogFilterMethod.OnResultExecuted, context));
            base.OnResultExecuted(context);
        }

        private void SendEventToService(LogFilterMethod method, ResultExecutedContext context)
        {
            var routeData = context.RouteData;
            var controllerType = context.Controller?.GetType();
            var controllerName = routeData.Values["controller"] as string;
            var actionName = routeData.Values["action"] as string;
            var actionMethod = context.Controller?.ViewBag?.RequestLog_ActionMethodInfo;
            var requestMethod = context?.RequestContext?.HttpContext?.Request?.HttpMethod;
            var url = context.HttpContext?.Request?.Url?.ToString();
            string result = context.Result?.ToString();
            int statusCode = context.HttpContext?.Response?.StatusCode ?? 0;

            RequestLogService.HandleActionEvent(new LogFilterEvent()
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
                SourceIP = context.Controller.ViewBag.RequestLog_SourceIP as string
        });
        }
    }
}
#endif
