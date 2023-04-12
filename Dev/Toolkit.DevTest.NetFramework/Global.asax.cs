using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.Metrics.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Modules.Metrics.Services;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Providers;
using QoDL.Toolkit.Dev.Common.Config;
using QoDL.Toolkit.Dev.Common.DataExport;
using QoDL.Toolkit.DevTest._TestImplementation.EndpointControl;
using QoDL.Toolkit.DevTest.Controllers;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Services;
using QoDL.Toolkit.Module.DataExport.Storage;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using QoDL.Toolkit.Module.EndpointControl.Results;
using QoDL.Toolkit.Module.EndpointControl.Services;
using QoDL.Toolkit.Module.EndpointControl.Storage;
using QoDL.Toolkit.Module.RequestLog.Util;
using QoDL.Toolkit.RequestLog.Enums;
using QoDL.Toolkit.RequestLog.Services;
using QoDL.Toolkit.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace QoDL.Toolkit.DevTest;

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
        TKMetricsUtil.AllowTrackRequestMetrics = (r) => true;
        DevConfig.ConfigureLocalAssetUrls("/dev");
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
    private static readonly ITKStringDictionaryStorage _settingsService = new TKFlatFileStringDictionaryStorage(@"C:\temp\settings.json");
    private static readonly TKMemoryMetricsStorage _memoryMetricsService = new();
    private static readonly ITKReleaseNotesProvider _releaseNotesProvider = new TKJsonFileReleaseNotesProvider(HostingEnvironment.MapPath(@"~\App_Data\ReleaseNotes.json"))
    {
        IssueUrlFactory = (id) => $"{"https://"}www.google.com/?q=Issue+{id}",
        IssueLinkTitleFactory = (id) => $"Jira {id}",
        PullRequestUrlFactory = (number) => $"{"https://"}www.google.com/?q=PR+{number}",
        //CommitShaUrlFactory = (sha) => $"{"https://"}github.com/chhjel/Toolkit/commit/{sha}"
    };

    private static readonly ITKDataExportService _dataExportService = new TKDataExportService(new ITKDataExportStream[]
    {
        new TestDataExportStreamQueryable(),
        new TestDataExportStreamEnumerableWithCustomParameters(),
        new TestDataExportStreamEnumerableWithQuery(),
        new TestDataExportStreamEnumerableWithQueryAndCustomParameters(),
        new TestDataExportStreamEnumerableWithoutInput(),
        new TestDataExportStreamHeavy()
    });
    private static readonly ITKDataExportPresetStorage _dataExportPresetStorage = new TKFlatFileDataExportPresetStorage(@"C:\temp\DataExportPreset.json");

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

        TKGlobalConfig.DefaultInstanceResolver = (type, scopeContainer) =>
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
            else if (type == typeof(ITKStringDictionaryStorage))
            {
                return _settingsService;
            }
            else if (type == typeof(ITKMetricsStorage))
            {
                return _memoryMetricsService;
            }
            else if (type == typeof(ITKReleaseNotesProvider))
            {
                return _releaseNotesProvider;
            }
            else if (type == typeof(ITKDataExportService))
            {
                return _dataExportService;
            }
            else if (type == typeof(ITKDataExportPresetStorage))
            {
                return _dataExportPresetStorage;
            }
            return null;
        };
    }
}
