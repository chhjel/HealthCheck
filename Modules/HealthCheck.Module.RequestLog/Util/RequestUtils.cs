#if NETFULL
using System;
using System.Net.Http;
using System.Web;
#endif
#pragma warning disable S2327

namespace HealthCheck.RequestLog.Util
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

        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        private const string OwinContext = "MS_OwinContext";

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

                // Web-hosting. Needs reference to System.Web.dll
                try
                {
                    if (request.Properties.ContainsKey(HttpContext))
                    {
                        dynamic ctx = request.Properties[HttpContext];
                        if (ctx != null)
                        {
                            if (ctx.Request.IsLocal)
                            {
                                return "localhost";
                            }

                            try
                            {
                                string ipAddress = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (!string.IsNullOrEmpty(ipAddress))
                                {
                                    string[] addresses = ipAddress.Split(',');
                                    if (addresses.Length != 0)
                                    {
                                        return addresses[0];
                                    }
                                }
                            }
                            catch (Exception) { /* Ignore errors here */ }

                            try
                            {
                                return ctx.Request.ServerVariables["REMOTE_ADDR"];
                            }
                            catch (Exception) { /* Ignore errors here */ }

                            return ctx.Request.UserHostAddress;
                        }
                    }

                    // Self-hosting. Needs reference to System.ServiceModel.dll.
                }
                catch (Exception) { /* Ignore errors here */ }

                try
                {
                    if (request.Properties.ContainsKey(RemoteEndpointMessage))
                    {
                        dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                        if (remoteEndpoint != null)
                        {
                            return remoteEndpoint.Address;
                        }
                    }
                }
                catch (Exception) { /* Ignore errors here */ }

                // Self-hosting using Owin. Needs reference to Microsoft.Owin.dll. 
                if (request.Properties.ContainsKey(OwinContext))
                {
                    dynamic owinContext = request.Properties[OwinContext];
                    if (owinContext != null)
                    {
                        return owinContext.Request.RemoteIpAddress;
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
        public static string GetIPAddress(HttpRequestBase request)
        {
            try
            {
                if (request == null)
                {
                    return null;
                }
                else if (request?.IsLocal == true)
                {
                    return "localhost";
                }

                string ipAddress = request.ServerVariables?["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0];
                    }
                }

                return request.ServerVariables?["REMOTE_ADDR"] ?? request?.UserHostAddress;
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif
    }
}
