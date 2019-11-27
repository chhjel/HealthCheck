#if NETFULL
using HealthCheck.Core.Abstractions;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HealthCheck.ActionLog.Services
{
    /// <summary>
    /// Can be used as fallback instead of dependency resolver.
    /// </summary>
    public static class TestLogServiceAccessor
    {
        /// <summary>
        /// Can be used as fallback instead of dependency resolver.
        /// </summary>
        public static ITestLogService Current { get; set; }

        internal static ITestLogService Locate()
        {
            if (Current != null)
            {
                return Current;
            }

            var service = DependencyResolver.Current?.GetServices<ITestLogService>()?.FirstOrDefault();
            if (service != null)
            {
                return service;
            }

            throw new ArgumentNullException($"Either ITestLogService must be registered in the dependency resolver or TestLogService.Current must be set.");
        }
    }
}
#endif
