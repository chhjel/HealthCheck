﻿using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Metrics.Services;
using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;
using HealthCheck.Core.Modules.ReleaseNotes.Providers;
using HealthCheck.Core.Modules.ReleaseNotes.Util;
using HealthCheck.DevTest._TestImplementation.EndpointControl;
using HealthCheck.DevTest.Controllers;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Results;
using HealthCheck.Module.EndpointControl.Services;
using HealthCheck.Module.EndpointControl.Storage;
using HealthCheck.Module.RequestLog.Util;
using HealthCheck.RequestLog.Enums;
using HealthCheck.RequestLog.Services;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
            HCMetricsUtil.AllowTrackRequestMetrics = (r) => true;
            HCMetricsUtil.SummaryHtmlJavascriptUrl = "/dev/GetMetricsScript";
            HCReleaseNotesUtil.SummaryHtmlJavascriptUrl = "/dev/GetReleaseNotesScript";
            SetupDummyIoC();
        }

        private static readonly FlatFileEndpointControlRequestHistoryStorage _endpointControlHistoryStorage
            = new(@"c:\temp\EC_History.json")
            {
                PrettyFormat = true
            };

        private static readonly FlatFileEndpointControlEndpointDefinitionStorage _endpointControlDefinitionStorage
            = new(@"c:\temp\EC_Definitions.json");

        private static readonly FlatFileEndpointControlRuleStorage _endpointControlRuleStorage
            = new(@"c:\temp\EC_Rules.json");
        private static readonly IHCStringDictionaryStorage _settingsService = new HCFlatFileStringDictionaryStorage(@"C:\temp\settings.json");
        private static readonly HCMemoryMetricsStorage _memoryMetricsService = new();
        private static readonly IHCReleaseNotesProvider _releaseNotesProvider = new HCJsonFileReleaseNotesProvider(HostingEnvironment.MapPath(@"~\App_Data\ReleaseNotes.json"))
            {
                IssueUrlFactory = (id) => $"{"https://"}www.google.com/?q=Issue+{id}",
                IssueLinkTitleFactory = (id) => $"Jira {id}",
                PullRequestUrlFactory = (number) => $"{"https://"}www.google.com/?q=PR+{number}",
                //CommitShaUrlFactory = (sha) => $"{"https://"}github.com/chhjel/HealthCheck/commit/{sha}"
            };

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
                    return new DefaultEndpointControlService(_endpointControlHistoryStorage, _endpointControlDefinitionStorage, _endpointControlRuleStorage)
                        .AddCustomBlockedResult(new CustomBlockedJsonResult())
                        .AddCustomBlockedResult(new EndpointControlForwardedRequestResult());
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
                else if (type == typeof(IHCStringDictionaryStorage))
                {
                    return _settingsService;
                }
                else if (type == typeof(IHCMetricsStorage))
                {
                    return _memoryMetricsService;
                }
                else if (type == typeof(IHCReleaseNotesProvider))
                {
                    return _releaseNotesProvider;
                }
                return null;
            };
        }
    }
}
