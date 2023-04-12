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
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
#endif

namespace QoDL.Toolkit.Web.Core.Utils;

/// <summary>
/// Utils related to http requests.
/// </summary>
public static class TKRequestUtils
{
    /// <summary>
    /// Value used instead of localhost IP.
    /// <para>Set to null to not use.</para>
    /// </summary>
    public static string LocalhostValue { get; set; } = "localhost";

    /// <summary>"CF-Connecting-IP"</summary>
    public const string ClientIPHeader_CF_ConnectingIP = "CF-Connecting-IP";
    /// <summary>"X-Forwarded-For"</summary>
    public const string ClientIPHeader_X_Forwarded_For = "X-Forwarded-For";
    /// <summary>"HTTP_X_FORWARDED_FOR"</summary>
    public const string ClientIPHeader_HTTP_Forwarded_For = "HTTP_X_FORWARDED_FOR";
    /// <summary>"REMOTE_ADDR"</summary>
    public const string ClientIPHeader_REMOTE_ADDR = "REMOTE_ADDR";

#if NETCORE || NETFULL
    /// <summary>
    /// Defaults to all built in headers in prioritized order. See available ClientIPHeader_* string constants on this class.
    /// </summary>
    public static List<string?>? DefaultClientIPHeaders { get; set; } = new()
    {
        ClientIPHeader_CF_ConnectingIP,
        ClientIPHeader_X_Forwarded_For,
        ClientIPHeader_HTTP_Forwarded_For,
        ClientIPHeader_REMOTE_ADDR
    };
#endif

#if NETCORE
    private static readonly List<Func<HttpContext?, List<string?>?, string?>> _ipResolvers = new()
    {
        (c, headers) => IsLocalRequest(c) ? LocalhostValue : null,
        (c, headers) => headers
            ?.Select(header => string.IsNullOrWhiteSpace(header) ? null : c?.Request?.Headers?[header])
            ?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
        (c, headers) => c?.Connection?.RemoteIpAddress?.ToString()
    };
#endif
#if NETFULL
    private static readonly List<Func<HttpRequestBase?, List<string?>?, string?>> _mvcIpResolvers = new()
    {
        (c, headers) => c?.IsLocal == true ? LocalhostValue : null,
        (c, headers) => headers
            ?.Select(header => string.IsNullOrWhiteSpace(header) ? null : c?.Headers?[header])
            ?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
        (c, headers) => c?.UserHostAddress?.ToString()
    };
#endif

#if NETCORE

    /// <summary>
    /// Attempt to get client IP address.
    /// </summary>
    public static string? GetIPAddress(HttpContext context, bool stripPortNumber = true, Func<HttpContext?, string?>? customResolver = null, List<string?>? headers = null)
    {
        try
        {
            var request = context?.Request;
            if (request == null)
            {
                return null;
            }

            headers ??= DefaultClientIPHeaders;
            return GetIPAddress(context, _ipResolvers, stripPortNumber, customResolver, headers);
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
    /// <summary>
    /// Attempt to get client IP address.
    /// <code>
    /// The first of the given that produces a value is used:
    /// 1. <paramref name="customResolver"/> if set.
    /// 2. <see cref="TKGlobalConfig.CurrentIPAddressResolver"/> if set.
    /// 3. First header with value in prioritized order from <paramref name="headers"/>. If left null <see cref="DefaultClientIPHeaders"/> is used.
    /// </code>
    /// </summary>
    public static string? GetIPAddress(HttpRequestBase request, bool stripPortNumber = true, Func<HttpRequestBase?, string?>? customResolver = null, List<string?>? headers = null)
    {
        try
        {
            if (request == null)
            {
                return null;
            }

            headers ??= DefaultClientIPHeaders;
            return GetIPAddress(request, _mvcIpResolvers, stripPortNumber, customResolver, headers);
        }
        catch (Exception)
        {
            return null;
        }
    }
#endif

#if NETCORE || NETFULL
    /// <summary>
    /// Internal. Use the other methods instead.
    /// </summary>
    public static string? GetIPAddress<TContext>(TContext context,
        List<Func<TContext?, List<string?>?, string?>> resolvers,
        bool stripPortNumber, Func<TContext?, string?>? customResolver,
        List<string?>? headers)
    {
        try
        {
            if (context == null) return null;

            try
            {
                if (customResolver != null)
                {
                    var customIP = customResolver?.Invoke(context);
                    if (stripPortNumber) customIP = customIP?.StripPortNumber();
                    if (!string.IsNullOrWhiteSpace(customIP))
                    {
                        return customIP;
                    }
                }
            }
            catch (Exception)
            {
                // Ignored
            }

            try
            {
                if (TKGlobalConfig.CurrentIPAddressResolver != null)
                {
                    var customIP = TKGlobalConfig.CurrentIPAddressResolver?.Invoke();
                    if (stripPortNumber) customIP = customIP?.StripPortNumber();
                    if (!string.IsNullOrWhiteSpace(customIP))
                    {
                        return customIP;
                    }
                }
            }
            catch (Exception)
            {
                // Ignored
            }

            string? ipAddress = null;
            foreach (var resolver in resolvers)
            {
                ipAddress = resolver?.Invoke(context, headers);
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
