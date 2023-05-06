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
    /// <summary></summary>
    public static string GetUrl(HttpRequest request) => request?.GetDisplayUrl();

    /// <summary></summary>
    public static string GetPath(HttpRequest request) => request?.Path;

    /// <summary></summary>
    public static string GetPathAndQuery(HttpRequest request) => request?.Path + request?.QueryString;

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

    public static string GetUrl(HttpRequestMessage request) => request?.RequestUri?.ToString();

    public static string GetUrl(HttpRequest request)
    {
        if (request == null || request.Url == null) return null;
        else if (request.RawUrl == null) return request.Url.ToString();

        var authority = request.Url.GetLeftPart(UriPartial.Authority);
        var path = request.RawUrl;
        return $"{authority}{path}";
    }

    public static string GetPath(HttpRequest request)
    {
        if (request == null || request.Url == null) return null;
        return request.Path;
    }

    public static string GetPathAndQuery(HttpRequest request)
    {
        if (request == null || request.Url == null) return null;
        return request.Path + request.QueryString;
    }

    public static string GetPath(HttpRequestMessage request)
    {
        if (request == null || request.RequestUri == null) return null;
        else return request.RequestUri.AbsolutePath;
    }

    public static string GetPathAndQuery(HttpRequestMessage request)
    {
        if (request == null || request.RequestUri == null) return null;
        else return request.RequestUri.PathAndQuery;
    }
#endif
}
