#if NETFULL
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using System;
using System.Web;

namespace QoDL.Toolkit.Module.IPWhitelist.Middleware;

/// <summary>
/// Handles blocking requests.
/// </summary>
public class TKIPWhitelistHttpModule : IHttpModule
{
    /// <summary></summary>
    public TKIPWhitelistHttpModule() { }

    /// <inheritdoc />
    public void Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(OnBeginRequest);
    }

    private static void OnBeginRequest(object sender, EventArgs eventArgs)
    {
        if (sender is not HttpApplication httpApplication || httpApplication?.Context == null)
        {
            return;
        }

        try
        {
            var service = TKIoCUtils.GetInstance<ITKIPWhitelistService>();
            if (service?.IsEnabled() != true) return;

            var context = httpApplication.Context;
            var result = TKAsyncUtils.RunSync(() => service.HandleRequestAsync(context.Request));
            if (!result.Blocked) return;

            context.Response.Clear();
            if (result.HttpStatusCode != null) context.Response.StatusCode = result.HttpStatusCode.Value;
            context.Response.Write(result.Response ?? string.Empty);
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKIPWhitelistHttpModule), nameof(OnBeginRequest), ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
#endif
