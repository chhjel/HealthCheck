#if NETFULL
using HealthCheck.Web.Core.Utils;
using HealthCheck.WebApi.Core.Utils;
using System;
using System.Net.Http;
using System.Web;
#endif

namespace HealthCheck.Module.RequestLog.Util
{
    internal static class RequestUtils
    {
#if NETFULL
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

        /// <summary>
        /// For WebAPI.
        /// </summary>
        public static string GetIPAddress(HttpRequestMessage request) => HCWebApiRequestUtils.GetIPAddress(request);

        /// <summary>
        /// For MVC.
        /// </summary>
        public static string GetIPAddress(HttpRequestBase request) => HCRequestUtils.GetIPAddress(request);
#endif
    }
}
