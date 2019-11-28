using HealthCheck.ActionLog.ActionFilters;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace HealthCheck.DevTest
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new TestLogActionFilter());
            filters.Add(new TestLogErrorFilter());
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new TestLogWebApiActionFilter());
            //filters.Add(new TestLogWebApiErrorFilter());
        }
    }
}
