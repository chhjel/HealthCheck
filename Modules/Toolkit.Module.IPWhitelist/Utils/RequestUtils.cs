#if NETFULL
using System;
using System.IO;
using System.Net.Http;
using System.Web;
using QoDL.Toolkit.WebApi.Core.Utils;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
#endif

using QoDL.Toolkit.Web.Core.Utils;

namespace QoDL.Toolkit.Module.IPWhitelist.Utils;

internal static class RequestUtils
{
#if NETCORE
    public static string GetUrl(HttpRequest request) => request?.GetDisplayUrl();

    /// <summary>
    /// For .Core.
    /// </summary>
    public static string GetIPAddress(HttpContext context) => TKRequestUtils.GetIPAddress(context);
#endif

#if NETFULL
    /// <summary>
    /// For MVC.
    /// </summary>
    public static string GetIPAddress(HttpRequestBase request) => TKRequestUtils.GetIPAddress(request);

    /// <summary>
    /// For WebAPI.
    /// </summary>
    public static string GetIPAddress(HttpRequestMessage request) => TKWebApiRequestUtils.GetIPAddress(request);

    public static byte[] ReadRequestBody(Stream input)
    {
        using MemoryStream ms = new();
        input.Seek(0, SeekOrigin.Begin);
        input.CopyTo(ms);
        input.Seek(0, SeekOrigin.Begin);
        return ms.ToArray();
    }

    public static string GetUrl(HttpRequestMessage request) => request?.RequestUri?.ToString();

    public static string GetUrl(HttpRequest request)
    {
        if (request == null || request.Url == null) return null;
        else if (request.RawUrl == null) return request.Url.ToString();

        var authority = request.Url.GetLeftPart(UriPartial.Authority);
        var path = request.RawUrl;
        return $"{authority}{path}";
    }

    public static string GetUrl(HttpRequestBase request)
    {
        if (request == null || request.Url == null) return null;
        else if (request.RawUrl == null) return request.Url.ToString();

        var authority = request.Url.GetLeftPart(UriPartial.Authority);
        var path = request.RawUrl;
        return $"{authority}{path}";
    }
#endif
}
