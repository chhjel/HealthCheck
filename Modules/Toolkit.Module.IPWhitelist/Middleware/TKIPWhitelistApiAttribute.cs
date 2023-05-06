#if NETFULL
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace QoDL.Toolkit.Module.IPWhitelist.Middleware;

/// <summary>
/// Blocks IP not matching the configured rules.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class TKIPWhitelistApiAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Check if the request is allowed and block it if not.
    /// </summary>
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        try
        {
            var service = TKIoCUtils.GetInstance<ITKIPWhitelistService>();
            if (service?.IsEnabled() != true) return;

            var request = actionContext.Request;
            var result = TKAsyncUtils.RunSync(() => service.HandleRequestAsync(request));
            if (!result.Blocked) return;

            actionContext.Response = actionContext.Request.CreateResponse(
                result.HttpStatusCode != null ? (HttpStatusCode)result.HttpStatusCode : HttpStatusCode.Forbidden,
                result.Response ?? string.Empty,
                "text/html");
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKIPWhitelistApiAttribute), nameof(OnActionExecuting), ex);
        }
    }
}
#endif
