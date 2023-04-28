using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Services;
using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Services;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Services;
using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Models;
using QoDL.Toolkit.Core.Modules.Dataflow.Services;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Services;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Notifiers;
using QoDL.Toolkit.Core.Modules.EventNotifications.Services;
using QoDL.Toolkit.Core.Modules.GoTo.Abstractions;
using QoDL.Toolkit.Core.Modules.GoTo.Services;
using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Services;
using QoDL.Toolkit.Core.Modules.LogViewer.Services;
using QoDL.Toolkit.Core.Modules.MappedData.Abstractions;
using QoDL.Toolkit.Core.Modules.MappedData.Services;
using QoDL.Toolkit.Core.Modules.MappedData.Utils;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Modules.Metrics.Services;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Providers;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.Settings.Abstractions;
using QoDL.Toolkit.Core.Modules.Settings.Services;
using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using QoDL.Toolkit.Core.Modules.SiteEvents.Services;
using QoDL.Toolkit.Dev.Common;
using QoDL.Toolkit.Dev.Common.Comparison;
using QoDL.Toolkit.Dev.Common.Config;
using QoDL.Toolkit.Dev.Common.ContentPermutation;
using QoDL.Toolkit.Dev.Common.DataExport;
using QoDL.Toolkit.Dev.Common.Dataflow;
using QoDL.Toolkit.Dev.Common.DataMapper;
using QoDL.Toolkit.Dev.Common.DataRepeater;
using QoDL.Toolkit.Dev.Common.EndpointControl;
using QoDL.Toolkit.Dev.Common.EventNotifier;
using QoDL.Toolkit.Dev.Common.GoTo;
using QoDL.Toolkit.Dev.Common.Jobs;
using QoDL.Toolkit.Dev.Common.Settings;
using QoDL.Toolkit.Dev.Common.Tests.Modules;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Services;
using QoDL.Toolkit.Module.DataExport.SQLExecutor;
using QoDL.Toolkit.Module.DataExport.Storage;
using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using QoDL.Toolkit.Module.DynamicCodeExecution.Storage;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Results;
using QoDL.Toolkit.Module.EndpointControl.Services;
using QoDL.Toolkit.Module.EndpointControl.Storage;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Services;
using QoDL.Toolkit.Module.IPWhitelist.Storage;
using QoDL.Toolkit.WebUI.Services;
using System;
using System.IO;

namespace QoDL.Toolkit.DevTest.NetCore_6._0.Config;

public static class IoCConfig
{
    internal static void Configure(IServiceCollection services, IWebHostEnvironment env)
    {
        // .Net Core things
        services.AddControllersWithViews()
            .AddSessionStateTempDataProvider();
        services.AddDistributedMemoryCache();
        services.AddSession();
        services.AddHttpContextAccessor();

        // TK things
        services.AddSingleton<IEndpointControlRuleStorage>((x) => new FlatFileEndpointControlRuleStorage(GetFilePath(@"App_Data\ec_rules.json", env)));
        services.AddSingleton<IEndpointControlEndpointDefinitionStorage>((x) => new FlatFileEndpointControlEndpointDefinitionStorage(GetFilePath(@"App_Data\ec_defs.json", env)));
        services.AddSingleton<IEndpointControlRequestHistoryStorage>((x) => new FlatFileEndpointControlRequestHistoryStorage(GetFilePath(@"App_Data\ec_history.json", env)));
        services.AddSingleton<IEndpointControlService>((s) =>
        {
            return new DefaultEndpointControlService(
                s.GetService<IEndpointControlRequestHistoryStorage>(),
                s.GetService<IEndpointControlEndpointDefinitionStorage>(),
                s.GetService<IEndpointControlRuleStorage>()
                )
                .AddCustomBlockedResult(new EndpointControlContentResult())
                .AddCustomBlockedResult(new EndpointControlRedirectResult())
                .AddPossibleCondition(new SimpleConditionA());
        });

        // Data Repeater
        services.AddSingleton<ITKDataRepeaterStream, TestOrderDataRepeaterStream>();
        services.AddSingleton<ITKDataRepeaterStream, TestXDataRepeaterStream>();
        services.AddSingleton<ITKDataRepeaterService, TKDataRepeaterService>();
        // Data Export
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamQueryable>();
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamEnumerableWithCustomParameters>();
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamEnumerableWithQuery>();
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamEnumerableWithQueryAndCustomParameters>();
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamEnumerableWithoutInput>();
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamHeavy>();
        services.AddSingleton<ITKSqlExportStreamQueryExecutor, TKDataExportExportSqlQueryExecutor>();
        services.AddSingleton<ITKDataExportStream, TestDataExportStreamSQL>();
        services.AddSingleton<ITKDataExportService, TKDataExportService>();
        services.AddSingleton<ITKDataExportPresetStorage>(x => new TKFlatFileDataExportPresetStorage(@"C:\temp\DataExportPreset.json"));

        // Permutations
        services.AddSingleton<ITKContentPermutationContentHandler, DevOrderPermutationHandler>();
        services.AddSingleton<ITKContentPermutationContentHandler, DevProductPermutationHandler>();
        services.AddSingleton<ITKContentPermutationContentDiscoveryService, TKContentPermutationContentDiscoveryService>();

        // Comparison
        services.AddSingleton<ITKComparisonTypeHandler, DevOrderComparisonTypeHandler>();
        services.AddSingleton<ITKComparisonTypeHandler, DummyComparisonTypeHandler>();
        services.AddSingleton<ITKComparisonDiffer, DevOrderDifferA>();
        services.AddSingleton<ITKComparisonDiffer, DisabledByDefaultDiffer>();
        services.AddSingleton<ITKComparisonDiffer, TKComparisonDifferSerializedJson>();
        services.AddSingleton<ITKComparisonService, TKComparisonService>();

        // GoTo
        services.AddSingleton<ITKGoToResolver, PotatoGotoResolver>();
        services.AddSingleton<ITKGoToResolver, ProductGotoResolver>();
        services.AddSingleton<ITKGoToService, TKGoToService>();

        // Messages
        services.AddSingleton<ITKMessageStorage>(x => new TKFlatFileMessageStore(@"c:\temp\tk_messages"));

        // Others
        RegisterDCEServices(services);
        services.AddSingleton(x => CreateSettingsService());
        services.AddSingleton(x => CreateSiteEventService(env));
        services.AddSingleton(x => CreateAuditEventService(env));
        services.AddSingleton(x => CreateLogSearcherService(env));
        services.AddSingleton(x => CreateAuditEventService(env));
        services.AddSingleton(x => CreateDataflowService());
        services.AddSingleton(x => CreateEventDataSinkService(x));
        services.AddSingleton<ISecureFileDownloadDefinitionStorage>(x => new FlatFileSecureFileDownloadDefinitionStorage(@"c:\temp\securefile_defs.json"));
        services.AddSingleton<IAccessManagerTokenStorage>(x => new FlatFileAccessManagerTokenStorage(@"C:\temp\AccessTokens.json"));
        services.AddSingleton<ITKMetricsStorage, TKMemoryMetricsStorage>();

        // MappedData
        TKMappedDataService.DisableCache = true;
        TKMappedDataUtils.SetExampleFor(new LeftRoot());
        services.AddSingleton<ITKMappedDataService, TKMappedDataService>();

        // Jobs
        services.AddSingleton<ITKJobsSource, DummyJobsSource>();
        services.AddSingleton<ITKJobsHistoryStorage, DummyJobsHistoryStorage>();
        services.AddSingleton<ITKJobsHistoryDetailsStorage, DummyJobsHistoryDetailsStorage>();
        services.AddSingleton<ITKJobsService, TKJobsService>();

        // IP Whitelist
        services.AddSingleton<ITKIPWhitelistService, TKIPWhitelistService>();
        services.AddSingleton<ITKIPWhitelistRuleStorage>(x => new TKIPWhitelistRuleFlatFileStorage(@"C:\temp\tk_ipwhitelist.json"));
        services.AddSingleton<ITKIPWhitelistConfigStorage>(x => new TKIPWhitelistConfigFlatFileStorage(@"C:\temp\tk_ipwhitelist_config.json"));

        services.AddSingleton<ITKReleaseNotesProvider>(new TKJsonFileReleaseNotesProvider(GetFilePath(@"App_Data\ReleaseNotes.json", env))
        {
            IssueUrlFactory = (id) => $"{"https://"}www.google.com/?q=Issue+{id}",
            IssueLinkTitleFactory = (id) => $"Jira {id}",
            PullRequestUrlFactory = (number) => $"{"https://"}www.google.com/?q=PR+{number}",
            //CommitShaUrlFactory = (sha) => $"{"https://"}github.com/chhjel/Toolkit/commit/{sha}"
        });

        TKMetricsUtil.AllowTrackRequestMetrics = (r) => true;
        DevConfig.ConfigureLocalAssetUrls();
    }

    private static string GetFilePath(string relativePath, IWebHostEnvironment env)
        => Path.GetFullPath(Path.Combine(env.ContentRootPath, relativePath));

    private static void RegisterDCEServices(IServiceCollection services)
    {
        services.AddSingleton<IDynamicCodeScriptStorage>(x => new FlatFileDynamicCodeScriptStorage(@"C:\temp\DCE_scripts.json"));
    }

    private static readonly TKFlatFileStringDictionaryStorage _settingsStorage = new(@"C:\temp\settings.json");
    private static ITKSettingsService CreateSettingsService()
        => new TKDefaultSettingsService(_settingsStorage);

    private static readonly FlatFileEventSinkNotificationConfigStorage _eventSinkNotificationConfigStorage
        = new(@"c:\temp\eventconfigs.json");
    private static readonly FlatFileEventSinkKnownEventDefinitionsStorage _eventSinkNotificationDefinitionStorage
        = new(@"c:\temp\eventconfig_defs.json");
    private static IEventDataSink CreateEventDataSinkService(IServiceProvider x)
    {
        var sink = new DefaultEventDataSink(_eventSinkNotificationConfigStorage, _eventSinkNotificationDefinitionStorage)
            .AddNotifier(new TKWebHookEventNotifier())
            .AddNotifier(new MyNotifier())
            .AddNotifier(new SimpleNotifier())
            .AddPlaceholder("NOW", () => DateTimeOffset.Now.ToString())
            .AddPlaceholder("ServerName", () => Environment.MachineName)
            .SetIsEnabled(() => x.GetRequiredService<ITKSettingsService>().GetSettings<TestSettings>().EnableEventRegistering);
        return sink;
    }

    public static TestStreamA TestStreamA => DataflowTests.TestStreamA;
    public static readonly TestStreamB TestStreamB = new();
    public static readonly TestStreamC TestStreamC = new();
    private static readonly SimpleStream _simpleStream = new("Simple A");
    private static readonly TestMemoryStream _memoryStream = new("Memory");
    private static readonly TestMemoryStream _otherStream1 = new(null);
    private static readonly TestMemoryStream _otherStream2 = new(null);
    private static readonly TestSearchABC _testSearchABC = new();
    private static readonly TestSearchABCGrouped _testSearchABCGrouped = new();
    private static IDataflowService<RuntimeTestAccessRole> CreateDataflowService()
    {
        return new DefaultDataflowService<RuntimeTestAccessRole>(new DefaultDataflowServiceOptions<RuntimeTestAccessRole>()
        {
            Streams = new IDataflowStream<RuntimeTestAccessRole>[]
            {
                TestStreamA,
                TestStreamB,
                TestStreamC,
                _simpleStream,
                _memoryStream,
                _otherStream1,
                _otherStream2
            },
            UnifiedSearches = new ITKDataflowUnifiedSearch<RuntimeTestAccessRole>[]
            {
                _testSearchABC,
                _testSearchABCGrouped
            }
        });
    }

    private static ILogSearcherService CreateLogSearcherService(IWebHostEnvironment env)
        => new FlatFileLogSearcherService(new FlatFileLogSearcherServiceOptions()
                .IncludeLogFilesInDirectory(GetFilePath(@"App_Data\TestLogs", env)));

    private static ISiteEventService CreateSiteEventService(IWebHostEnvironment env)
        => new SiteEventService(new FlatFileSiteEventStorage(
            GetFilePath(@"App_Data\SiteEventStorage.json", env), maxEventAge: TimeSpan.FromDays(5), delayFirstCleanup: false));

    private static IAuditEventStorage CreateAuditEventService(IWebHostEnvironment env)
    {
        var blobFolder = GetFilePath(@"App_Data\AuditEventBlobs", env);
        var blobService = new FlatFileAuditBlobStorage(blobFolder, maxEventAge: TimeSpan.FromDays(1));
        return new FlatFileAuditEventStorage(GetFilePath(@"App_Data\AuditEventStorage.json", env),
            maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false, blobStorage: blobService);
    }
}
