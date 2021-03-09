using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.RequestLog.ActionFilters;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace HealthCheck.DevTest
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequestLogActionFilterAttribute());
            filters.Add(new RequestLogErrorFilterAttribute());
            filters.Add(new HandleErrorAttribute());
            filters.Add(new HCControlledEndpointAttribute());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new RequestLogWebApiActionFilterAttribute());
        }
    }
}
