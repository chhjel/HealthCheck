﻿using HealthCheck.ActionLog.Enums;
using HealthCheck.ActionLog.Services;
using HealthCheck.ActionLog.Util;
using HealthCheck.DevTest.Controllers;
using System.Linq;
using System.Threading.Tasks;
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
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            TestLogServiceAccessor.Current = new TestLogService(
                new FlatFileTestLogStorage(@"c:\temp\TestLogStore.json"),
                new TestLogServiceOptions
                {
                    MaxCallCount = 3,
                    MaxErrorCount = 5,
                    CallStoragePolicy = TestLogCallStoragePolicy.RemoveOldest,
                    ErrorStoragePolicy = TestLogCallStoragePolicy.RemoveOldest,
                    ControllerGroupNameFactory = (ctype) =>
                    {
                        var ns = ctype.Namespace;
                        var lastNsPart = ns.Split('.').Last();
                        return (lastNsPart.ToLower().StartsWith("controller"))
                            ? null
                            : lastNsPart;
                    }
                });

            if (TestLogServiceAccessor.Current != null)
            {
                Task.Run(() => TestLogUtil.EnsureDefinitionsFromTypes(TestLogServiceAccessor.Current, new[] { typeof(DevController).Assembly }));
            }
        }
    }
}
