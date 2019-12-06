using HealthCheck.RequestLog.ActionFilters;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace HealthCheck.DevTest
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequestLogActionFilter());
            filters.Add(new RequestLogErrorFilter());
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new RequestLogWebApiActionFilter());
        }
    }
}
