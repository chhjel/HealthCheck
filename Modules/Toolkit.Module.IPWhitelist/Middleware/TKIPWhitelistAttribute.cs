#if NETFULL
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using System;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace QoDL.Toolkit.Module.IPWhitelist.Middleware;

/// <summary>
/// Blocks IP not matching the configured rules.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class TKIPWhitelistAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Check if the request is allowed and block it if not.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        try
        {
            var service = TKIoCUtils.GetInstance<ITKIPWhitelistService>();
            if (service?.IsEnabled() != true) return;

            var request = filterContext.HttpContext.ApplicationInstance.Context.Request;
            var result = TKAsyncUtils.RunSync(() => service.HandleRequestAsync(request));
            if (!result.Blocked) return;

            filterContext.HttpContext.Response.StatusCode = (int)(result.HttpStatusCode != null ? result.HttpStatusCode : (int)HttpStatusCode.Forbidden);
            filterContext.Result = new ContentResult {
                Content = result.Response ?? string.Empty,
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8
            };
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKIPWhitelistAttribute), nameof(OnActionExecuting), ex);
        }
    }
}
#endif
