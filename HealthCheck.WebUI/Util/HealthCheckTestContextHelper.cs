using HealthCheck.Core.Modules.Tests.Models;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
using HealthCheck.Core.Util;
#endif

namespace HealthCheck.WebUI.Util
{
    internal static class HealthCheckTestContextHelper
    {
        public static bool CurrentRequestIsTest
        {
            get
            {
                var key = "___hc_request_is_test";
#if NETFULL
                return HttpContext.Current?.Items?[key] is bool value && value;
#elif NETCORE
                var httpContext = HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
                return httpContext?.Items?[key] is bool value && value;
#else
                _ = key;
                return false;
#endif
            }
            set
            {
                var key = "___hc_request_is_test";
#if NETFULL
                var items = HttpContext.Current?.Items;
                if (items != null)
                {
                    items[key] = value;
                }
#elif NETCORE
                var httpContext = HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
                var items = httpContext?.Items;
                if (items != null)
                {
                    items[key] = value;
                }
#else
                _ = key;
                _ = value;
#endif
            }
        }

        public static HCTestContext GetCurrentTestContext()
        {
            var key = "___hc_test_context";

            HCTestContext context = null;
#if NETFULL
            var items = HttpContext.Current?.Items;
            if (items != null)
            {
                context = items[key] as HCTestContext;
                if (context == null)
                {
                    context = ContextFactory();
                    items[key] = context;
                }
            }
#elif NETCORE
            var httpContext = HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
            var items = httpContext?.Items;
            if (items != null)
            {
                context = items[key] as HCTestContext;
                if (context == null)
                {
                    context = ContextFactory();
                    items[key] = context;
                }
            }
#else
            _ = key + nameof(ContextFactory);
#endif
            return context;
        }

        private static HCTestContext ContextFactory() => new();
    }
}
