#if NETFULL
using QoDL.Toolkit.Module.RequestLog.Util;
using QoDL.Toolkit.RequestLog.Abstractions;
using QoDL.Toolkit.RequestLog.Enums;
using QoDL.Toolkit.RequestLog.Models;
using QoDL.Toolkit.RequestLog.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace QoDL.Toolkit.RequestLog.ActionFilters;

/// <summary>
/// Intercepts and logs successfull MVC requests.
/// </summary>
public class RequestLogActionFilterAttribute : ActionFilterAttribute
{
    private IRequestLogService RequestLogService => RequestLogServiceAccessor.Current;

    /// <summary>
    /// Intercepts and logs successfull MVC requests.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // TaskAsyncActionDescriptor
        if (filterContext.Controller?.ViewBag != null)
        {
            MethodInfo actionMethod = null;
            if (filterContext.ActionDescriptor is ReflectedActionDescriptor reflectedActionDescriptor)
            {
                actionMethod = reflectedActionDescriptor?.MethodInfo;
            }
            else if (filterContext.ActionDescriptor is TaskAsyncActionDescriptor taskAsyncActionDescriptor)
            {
                actionMethod = taskAsyncActionDescriptor?.TaskMethodInfo;
            }
            filterContext.Controller.ViewBag.RequestLog_ActionMethodInfo = actionMethod;
            filterContext.Controller.ViewBag.RequestLog_SourceIP = RequestUtils.GetIPAddress(filterContext?.HttpContext?.Request);
        }
        base.OnActionExecuting(filterContext);
    }

    /// <summary>
    /// Intercepts and logs successfull MVC requests.
    /// </summary>
    public override void OnResultExecuted(ResultExecutedContext filterContext)
    {
        Task.Run(() => SendEventToService(LogFilterMethod.OnResultExecuted, filterContext));
        base.OnResultExecuted(filterContext);
    }

    private void SendEventToService(LogFilterMethod method, ResultExecutedContext context)
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
            string result = context.Result?.ToString();
            int statusCode = context.HttpContext?.Response?.StatusCode ?? 0;

            RequestLogService.HandleRequestEvent(new LogFilterEvent()
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
        catch (Exception) { /* Ignore errors here */ }
    }
}
#endif
