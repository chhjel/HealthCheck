#if NETFULL
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace QoDL.Toolkit.WebApi.Core.Utils;

/// <summary>
/// Utils related to http requests for WebApi.
/// </summary>
public static class TKWebApiRequestUtils
{
    private static readonly List<Func<HttpRequestMessage?, List<string?>?, string?>> _webApiIpResolvers = new()
    {
        ResolveLocalAndHeadersAndUserHostAddress,
        ResolveServiceModelRemiteEndpointAddress,
        ResolveFromOwinContext
    };

    /// <summary>
    /// Attempt to get client IP address.
    /// <code>
    /// The first of the given that produces a value is used:
    /// 1. <paramref name="customResolver"/> if set.
    /// 2. <see cref="TKGlobalConfig.CurrentIPAddressResolver"/> if set.
    /// 3. First header with value in prioritized order from <paramref name="headers"/>. If left null <see cref="TKRequestUtils.DefaultClientIPHeaders"/> is used.
    /// </code>
    /// </summary>
    public static string? GetIPAddress(HttpRequestMessage request, bool stripPortNumber = true, Func<HttpRequestMessage?, string?>? customResolver = null, List<string?>? headers = null)
    {
        try
        {
            if (request == null)
            {
                return null;
            }

            headers ??= TKRequestUtils.DefaultClientIPHeaders;
            return TKRequestUtils.GetIPAddress(request, _webApiIpResolvers, stripPortNumber, customResolver, headers);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? ResolveLocalAndHeadersAndUserHostAddress(HttpRequestMessage? request, List<string?>? headers)
    {
        // Web-hosting. Needs reference to System.Web.dll
        const string _httpContext = "MS_HttpContext";
        try
        {
            if (request == null)
            {
                return null;
            }

            if (request.Properties.ContainsKey(_httpContext))
            {
                dynamic ctx = request.Properties[_httpContext];
                if (ctx != null)
                {
                    if (ctx.Request.IsLocal)
                    {
                        return TKRequestUtils.LocalhostValue;
                    }

                    try
                    {
                        return headers
                            ?.Select(header => string.IsNullOrWhiteSpace(header) ? null : ctx?.Request?.ServerVariables?[header])
                            ?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
                    }
                    catch (Exception) { /* Ignore errors here */ }

                    return ctx.Request.UserHostAddress as string;
                }
            }

        }
        catch (Exception) { /* Ignore errors here */ }

        return null;
    }

    private static string? ResolveServiceModelRemiteEndpointAddress(HttpRequestMessage? request, List<string?>? headers)
    {
        // Self-hosting. Needs reference to System.ServiceModel.dll.
        const string _remoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        try
        {
            if (request == null)
            {
                return null;
            }

            try
            {
                if (request.Properties.ContainsKey(_remoteEndpointMessage))
                {
                    dynamic remoteEndpoint = request.Properties[_remoteEndpointMessage];
                    if (remoteEndpoint != null)
                    {
                        return remoteEndpoint.Address as string;
                    }
                }
            }
            catch (Exception) { /* Ignore errors here */ }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? ResolveFromOwinContext(HttpRequestMessage? request, List<string?>? headers)
    {
        // Self-hosting using Owin. Needs reference to Microsoft.Owin.dll. 
        const string _owinContext = "MS_OwinContext";
        try
        {
            if (request == null)
            {
                return null;
            }

            if (request.Properties.ContainsKey(_owinContext))
            {
                dynamic owinContext = request.Properties[_owinContext];
                if (owinContext != null)
                {
                    return owinContext.Request.RemoteIpAddress as string;
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
#endif
