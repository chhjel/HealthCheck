#if NETFULL
using HealthCheck.ActionLog.Services;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.ActionsTestLog.Enums;
using HealthCheck.Core.Modules.ActionsTestLog.Models;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace HealthCheck.ActionLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs successfull MVC requests.
    /// </summary>
    public class TestLogActionFilter : ActionFilterAttribute
    {
        private ITestLogService TestLogService => TestLogServiceAccessor.Current;

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
                context.Controller.ViewBag.ActionsTestLog_ActionMethodInfo = actionMethod;
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
            var actionMethod = context.Controller?.ViewBag?.ActionsTestLog_ActionMethodInfo;
            var requestMethod = context?.RequestContext?.HttpContext?.Request?.HttpMethod;
            var url = context.HttpContext?.Request?.Url?.ToString();
            string result = context.Result?.ToString();
            int statusCode = context.HttpContext?.Response?.StatusCode ?? 0;

            TestLogService.HandleActionEvent(new LogFilterEvent()
            {
                FilterMethod = method,
                ControllerType = controllerType,
                Controller = controllerName,
                Action = actionName,
                ActionMethod = actionMethod,
                RequestMethod = requestMethod,
                Url = url,
                Result = result,
                StatusCode = statusCode.ToString()
            });
        }
    }
}
#endif
