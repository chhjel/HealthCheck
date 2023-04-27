#if NETCORE
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.IPWhitelist.Middleware;

/// <summary>
/// Handles blocking requests.
/// </summary>
public class TKIPWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITKIPWhitelistService _whitelistService;

    /// <summary></summary>
    public TKIPWhitelistMiddleware(RequestDelegate next, ITKIPWhitelistService whitelistService)
    {
        _next = next;
        _whitelistService = whitelistService;
    }

    /// <summary></summary>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            if (_whitelistService?.IsEnabled() == true)
            {
                var result = await _whitelistService.HandleRequestAsync(context);
                if (result.Blocked)
                {
                    context.Response.Clear();
                    if (result.HttpStatusCode != null) context.Response.StatusCode = result.HttpStatusCode.Value;
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(result.Response ?? string.Empty);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKIPWhitelistMiddleware), nameof(Invoke), ex);
        }

        await _next.Invoke(context);
    }
}

/// <summary></summary>
public static class TKIPWhitelistMiddlewareExtensions
{
    /// <summary></summary>
    public static IApplicationBuilder UseTKIPWhitelistMiddleware(this IApplicationBuilder builder)
        => builder.UseMiddleware<TKIPWhitelistMiddleware>();
}
#endif
