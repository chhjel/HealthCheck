using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.DevTest.Controllers;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Services;
using HealthCheck.Module.EndpointControl.Storage;
using HealthCheck.RequestLog.Enums;
using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using static HealthCheck.DevTest.Controllers.DevController;

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
            SetupDummyIoC();
        }

        private static readonly FlatFileEndpointControlRequestHistoryStorage _endpointControlHistoryStorage
            = new FlatFileEndpointControlRequestHistoryStorage(@"c:\temp\EC_History.json")
            {
                PrettyFormat = true
            };

        private static readonly FlatFileEndpointControlEndpointDefinitionStorage _endpointControlDefinitionStorage
            = new FlatFileEndpointControlEndpointDefinitionStorage(@"c:\temp\EC_Definitions.json");

        private static readonly FlatFileEndpointControlRuleStorage _endpointControlRuleStorage
            = new FlatFileEndpointControlRuleStorage(@"c:\temp\EC_Rules.json");
        private readonly IHealthCheckSettingsService _settingsService = new FlatFileHealthCheckSettingsService<TestSettings>(@"C:\temp\settings.json");

        private void SetupDummyIoC()
        {
            if (!_endpointControlRuleStorage.GetRules().Any())
            {
                _endpointControlRuleStorage.InsertRule(new EndpointControlRule
                {
                    Enabled = true,
                    CurrentEndpointRequestCountLimits = new List<EndpointControlCountOverDuration>
                    {
                        new EndpointControlCountOverDuration() { Duration = TimeSpan.FromSeconds(10), Count = 5 }
                    },
                    TotalRequestCountLimits = new List<EndpointControlCountOverDuration>(),
                    EndpointIdFilter = new EndpointControlPropertyFilter(),
                    UserAgentFilter = new EndpointControlPropertyFilter(),
                    UrlFilter = new EndpointControlPropertyFilter(),
                    UserLocationIdFilter = new EndpointControlPropertyFilter()
                });
            }

            HCGlobalConfig.DefaultInstanceResolver = (type) =>
            {
                if (type == typeof(IEndpointControlService))
                {
                    return new DefaultEndpointControlService(_endpointControlHistoryStorage, _endpointControlDefinitionStorage, _endpointControlRuleStorage);
                }
                else if (type == typeof(IEndpointControlRuleStorage))
                {
                    return _endpointControlRuleStorage;
                }
                else if (type == typeof(IEndpointControlEndpointDefinitionStorage))
                {
                    return _endpointControlDefinitionStorage;
                }
                else if (type == typeof(IEndpointControlRequestHistoryStorage))
                {
                    return _endpointControlHistoryStorage;
                }
                else if (type == typeof(IHealthCheckSettingsService))
                {
                    return _settingsService;
                }
                return null;
            };
        }
    }
}
