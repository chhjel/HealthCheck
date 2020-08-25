﻿using HealthCheck.DevTest.Controllers;
using HealthCheck.RequestLog.Enums;
using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace HealthCheck.DevTest
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            FilterConfig.RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            RequestLogServiceAccessor.Current = new RequestLogService(
                new FlatFileRequestLogStorage(HostingEnvironment.MapPath("~/App_Data/RequestLog.json")),
                new RequestLogServiceOptions
                {
                    MaxCallCount = 3,
                    MaxErrorCount = 5,
                    CallStoragePolicy = RequestLogCallStoragePolicy.RemoveOldest,
                    ErrorStoragePolicy = RequestLogCallStoragePolicy.RemoveOldest,
                });

            RequestLogUtil.EnsureDefinitionsFromTypes(RequestLogServiceAccessor.Current, new[] { typeof(DevController).Assembly });
        }
    }
}