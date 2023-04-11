#if NETFULL
using QoDL.Toolkit.RequestLog.Abstractions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QoDL.Toolkit.RequestLog.Services;

/// <summary>
/// Can be used as fallback instead of dependency resolver.
/// </summary>
public static class RequestLogServiceAccessor
{
    /// <summary>
    /// Can be used as fallback instead of dependency resolver.
    /// </summary>
    public static IRequestLogService Current { get; set; }

    internal static IRequestLogService Locate()
    {
        if (Current != null)
        {
            return Current;
        }

        var service = DependencyResolver.Current?.GetServices<IRequestLogService>()?.FirstOrDefault();
        if (service != null)
        {
            return service;
        }

        throw new ArgumentException($"Either IRequestLogService must be registered in the dependency resolver or RequestLogService.Current must be set.");
    }
}
#endif
