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
#endif
#if NETCORE
            var httpContext = IoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
            return httpContext?.Items?[key] is bool value && value;
#endif
                return false;
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
#endif
#if NETCORE
                var httpContext = IoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
                var items = httpContext?.Items;
                if (items != null)
                {
                    items[key] = value;
                }
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
#endif
#if NETCORE
            var httpContext = IoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
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
#endif
            return context;
        }

        private static HCTestContext ContextFactory()
        {
            return new HCTestContext();
        }
    }
}
