﻿#if NETFULL
using HealthCheck.Core.Abstractions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HealthCheck.RequestLog.Services
{
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

            throw new ArgumentNullException($"Either IRequestLogService must be registered in the dependency resolver or RequestLogService.Current must be set.");
        }
    }
}
#endif