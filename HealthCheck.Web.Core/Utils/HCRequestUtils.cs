#if NETFULL
using System;
using System.Web;
using System.Net.Http;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
#endif

#if NETCORE || NETFULL
using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
#endif

namespace HealthCheck.Web.Core.Utils
{
    /// <summary>
    /// Utils related to http requests.
    /// </summary>
    public static class HCRequestUtils
    {
        /// <summary>
        /// Value used instead of localhost IP.
        /// </summary>
        public static string LocalhostValue { get; set; } = "localhost";

#if NETCORE
        private static readonly List<Func<HttpContext?, string?>> _ipResolvers = new()
        {
            (c) => IsLocalRequest(c) ? LocalhostValue : null,
            (c) => c?.Request?.Headers?["X-Forwarded-For"],
            (c) => c?.Request?.Headers?["HTTP_X_FORWARDED_FOR"],
            (c) => c?.Request?.Headers?["REMOTE_ADDR"],
            (c) => c?.Connection?.RemoteIpAddress?.ToString()
        };

        /// <summary>
        /// Attempt to get client IP address.
        /// </summary>
        public static string? GetIPAddress(HttpContext context, bool stripPortNumber = true)
        {
            try
            {
                var request = context?.Request;
                if (request == null)
                {
                    return null;
                }

                return GetIPAddress(context, _ipResolvers, stripPortNumber);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Check if the request is local.
        /// </summary>
        public static bool IsLocalRequest(HttpContext? context)
        {
            if (context?.Connection?.RemoteIpAddress == null && context?.Connection?.LocalIpAddress == null)
            {
                return true;
            }
            else if (context?.Connection?.RemoteIpAddress?.Equals(context?.Connection?.LocalIpAddress) == true)
            {
                return true;
            }
            else if (context?.Connection?.RemoteIpAddress != null && IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
            {
                return true;
            }
            return false;
        }
#endif

#if NETFULL
        private static readonly List<Func<HttpRequestBase?, string?>> _mvcIpResolvers = new()
        {
            (c) => c?.IsLocal == true ? LocalhostValue : null,
            (c) => c?.Headers?["X-Forwarded-For"],
            (c) => c?.Headers?["HTTP_X_FORWARDED_FOR"],
            (c) => c?.Headers?["REMOTE_ADDR"],
            (c) => c?.UserHostAddress?.ToString()
        };

        /// <summary>
        /// Attempt to get client IP address.
        /// </summary>
        public static string? GetIPAddress(HttpRequestBase request, bool stripPortNumber = true)
        {
            try
            {
                if (request == null)
                {
                    return null;
                }

                return GetIPAddress(request, _mvcIpResolvers, stripPortNumber);
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif

#if NETCORE || NETFULL
        private static string? GetIPAddress<TContext>(TContext context, List<Func<TContext, string?>> resolvers, bool stripPortNumber)
        {
            try
            {
                try
                {
                    var customIP = HCGlobalConfig.CurrentIPAddressResolver?.Invoke();
                    if (stripPortNumber) customIP = customIP?.StripPortNumber();
                    if (!string.IsNullOrWhiteSpace(customIP))
                    {
                        return customIP;
                    }
                }
                catch (Exception)
                {
                    // Ignored
                }

                string? ipAddress = null;
                foreach (var resolver in resolvers)
                {
                    ipAddress = resolver?.Invoke(context);
                    if (!string.IsNullOrWhiteSpace(ipAddress)) break;
                }

#if NETCORE
                if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(','))
#elif NETFULL
                if (ipAddress != null && !string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(','))
#endif
                {
                    string[] addresses = ipAddress.Split(',');
                    if (!string.IsNullOrWhiteSpace(addresses[0]))
                    {
                        ipAddress = addresses[0];
                    }
                }

                if (stripPortNumber) ipAddress = ipAddress?.StripPortNumber();
                return ipAddress;
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif
            }
}
