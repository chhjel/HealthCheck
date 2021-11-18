#if NETFULL
using System.IO;
using System.Net.Http;
using System.Web;
#endif
#if NETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
#endif
using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using System;

namespace HealthCheck.Module.EndpointControl.Utils
{
    internal static class RequestUtils
    {
#if NETFULL
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
                                string ipAddress = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
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
                                return ctx.Request.ServerVariables["REMOTE_ADDR"]?.StripPortNumber();
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
        public static string GetIPAddress(HttpRequestBase request)
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

                if (request?.IsLocal == true)
                {
                    return "localhost";
                }

                string ipAddress = request.ServerVariables?["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0]?.StripPortNumber();
                    }
                }

                ipAddress = request.ServerVariables?["REMOTE_ADDR"] ?? request?.UserHostAddress;
                return ipAddress?.StripPortNumber();
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif

#if NETCORE
        public static string GetUrl(HttpRequest request) => request?.GetDisplayUrl();

        /// <summary>
        /// For .Core.
        /// </summary>
        public static string GetIPAddress(HttpContext context)
        {
            try
            {
                var request = context?.Request;

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

                if (IsLocalRequest(context))
                {
                    return "localhost";
                }

                string ipAddress = request?.Headers?["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0]?.StripPortNumber();
                    }
                }

                ipAddress = request?.Headers?["REMOTE_ADDR"] ?? context?.Connection?.RemoteIpAddress?.ToString();
                return ipAddress?.StripPortNumber();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsLocalRequest(HttpContext context)
        {
            if (context?.Connection?.RemoteIpAddress == null && context?.Connection?.LocalIpAddress == null)
            {
                return true;
            }
            else if (context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress))
            {
                return true;
            }
            else if (IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
            {
                return true;
            }
            return false;
        }
#endif
    }
}
