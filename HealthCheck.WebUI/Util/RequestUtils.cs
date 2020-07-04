#if NETFULL
using System;
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
#endif

namespace HealthCheck.WebUI.Util
{
    internal static class RequestUtils
    {
#if NETFULL
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

#if NETCORE
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
                else if (IsLocalRequest(context))
                {
                    return "localhost";
                }

                string ipAddress = request?.Headers?["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0];
                    }
                }

                return request?.Headers?["REMOTE_ADDR"] ?? context?.Connection?.RemoteIpAddress?.ToString();
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
