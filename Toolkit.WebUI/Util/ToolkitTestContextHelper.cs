using QoDL.Toolkit.Core.Modules.Tests.Models;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Core.Util;
#endif

namespace QoDL.Toolkit.WebUI.Util
{
    internal static class ToolkitTestContextHelper
    {
        public static bool CurrentRequestIsTest
        {
            get
            {
                var key = "___tk_request_is_test";
#if NETFULL
                return HttpContext.Current?.Items?[key] is bool value && value;
#elif NETCORE
                var httpContext = TKIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
                return httpContext?.Items?[key] is bool value && value;
#else
                _ = key;
                return false;
#endif
            }
            set
            {
                var key = "___tk_request_is_test";
#if NETFULL
                var items = HttpContext.Current?.Items;
                if (items != null)
                {
                    items[key] = value;
                }
#elif NETCORE
                var httpContext = TKIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
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

        public static TKTestContext GetCurrentTestContext()
        {
            var key = "___tk_test_context";

            TKTestContext context = null;
#if NETFULL
            var items = HttpContext.Current?.Items;
            if (items != null)
            {
                context = items[key] as TKTestContext;
                if (context == null)
                {
                    context = ContextFactory();
                    items[key] = context;
                }
            }
#elif NETCORE
            var httpContext = TKIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext;
            var items = httpContext?.Items;
            if (items != null)
            {
                context = items[key] as TKTestContext;
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

        private static TKTestContext ContextFactory() => new();
    }
}
