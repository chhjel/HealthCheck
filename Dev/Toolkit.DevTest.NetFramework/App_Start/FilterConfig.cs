using QoDL.Toolkit.Module.EndpointControl.Attributes;
using QoDL.Toolkit.Module.IPWhitelist.Middleware;
using QoDL.Toolkit.RequestLog.ActionFilters;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace QoDL.Toolkit.DevTest;

public static class FilterConfig
{
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
        filters.Add(new RequestLogActionFilterAttribute());
        filters.Add(new RequestLogErrorFilterAttribute());
        filters.Add(new TKControlledEndpointAttribute());
        filters.Add(new TKIPWhitelistAttribute());
        filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterWebApiFilters(HttpFilterCollection filters)
    {
        filters.Add(new TKControlledApiEndpointAttribute());
        filters.Add(new TKIPWhitelistApiAttribute());
        filters.Add(new RequestLogWebApiActionFilterAttribute());
    }
}
