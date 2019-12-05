#if NETFULL
using HealthCheck.ActionLog.Services;
using HealthCheck.ActionLog.Util;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.ActionsTestLog.Enums;
using HealthCheck.Core.Modules.ActionsTestLog.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HealthCheck.ActionLog.ActionFilters
{
    /// <summary>
    /// Intercepts and logs unhandled MVC exceptions.
    /// </summary>
    public class TestLogErrorFilter : HandleErrorAttribute
    {
        private ITestLogService TestLogService => TestLogServiceAccessor.Current;

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
            var routeData = context.RouteData;
            var controllerType = context.Controller?.GetType();
            var controllerName = routeData.Values["controller"] as string;
            var actionName = routeData.Values["action"] as string;
            var actionMethod = context.Controller?.ViewBag?.ActionsTestLog_ActionMethodInfo;
            var requestMethod = context?.RequestContext?.HttpContext?.Request?.HttpMethod;
            var url = context.HttpContext?.Request?.Url?.ToString();
            int statusCode = (context.Exception != null)
                ? new HttpException(null, context.Exception).GetHttpCode()
                : context.HttpContext?.Response?.StatusCode ?? 500;

            TestLogService.HandleActionEvent(new LogFilterEvent()
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
    }
}
#endif
