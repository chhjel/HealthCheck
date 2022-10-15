﻿#if NETFULL
using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using HealthCheck.Web.Core.Utils;
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

        private const string _httpContext = "MS_HttpContext";
        private const string _remoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        private const string _owinContext = "MS_OwinContext";

        /// <summary>
        /// For WebAPI.
        /// </summary>
        public static string GetIPAddress(HttpRequestMessage request)
        {
            try
            {
                if (request == null)
                {
                    return null;
                }

                try
                {
                    var customIP = HCGlobalConfig.CurrentIPAddressResolver?.Invoke()?.StripPortNumber();
                    if (!string.IsNullOrWhiteSpace(customIP))
                    {
                        return customIP;
                    }
                }
                catch (Exception)
                {
                    // Ignored
                }

                // Web-hosting. Needs reference to System.Web.dll
                try
                {
                    if (request.Properties.ContainsKey(_httpContext))
                    {
                        dynamic ctx = request.Properties[_httpContext];
                        if (ctx != null)
                        {
                            if (ctx.Request.IsLocal)
                            {
                                return "localhost";
                            }

                            try
                            {
                                string ipAddress = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]
                                    ?? ctx?.request?.ServerVariables?["X-Forwarded-For"];
                                if (!string.IsNullOrEmpty(ipAddress))
                                {
                                    string[] addresses = ipAddress.Split(',');
                                    if (addresses.Length != 0)
                                    {
                                        return addresses[0]?.StripPortNumber();
                                    }
                                }
                            }
                            catch (Exception) { /* Ignore errors here */ }

                            try
                            {
                                return (ctx.Request.ServerVariables["REMOTE_ADDR"] as string)?.StripPortNumber();
                            }
                            catch (Exception) { /* Ignore errors here */ }

                            return (ctx.Request.UserHostAddress as string)?.StripPortNumber();
                        }
                    }

                    // Self-hosting. Needs reference to System.ServiceModel.dll.
                }
                catch (Exception) { /* Ignore errors here */ }

                try
                {
                    if (request.Properties.ContainsKey(_remoteEndpointMessage))
                    {
                        dynamic remoteEndpoint = request.Properties[_remoteEndpointMessage];
                        if (remoteEndpoint != null)
                        {
                            return (remoteEndpoint.Address as string)?.StripPortNumber();
                        }
                    }
                }
                catch (Exception) { /* Ignore errors here */ }

                // Self-hosting using Owin. Needs reference to Microsoft.Owin.dll. 
                if (request.Properties.ContainsKey(_owinContext))
                {
                    dynamic owinContext = request.Properties[_owinContext];
                    if (owinContext != null)
                    {
                        return (owinContext.Request.RemoteIpAddress as string)?.StripPortNumber();
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// For MVC.
        /// </summary>
        public static string GetIPAddress(HttpRequestBase request) => HCRequestUtils.GetIPAddress(request);
#endif
    }
}
