using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Services;
using HealthCheck.Core.Modules.Comparison.Abstractions;
using HealthCheck.Core.Modules.Comparison.Services;
using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Services;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Modules.Dataflow.Services;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Services;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Notifiers;
using HealthCheck.Core.Modules.EventNotifications.Services;
using HealthCheck.Core.Modules.GoTo.Abstractions;
using HealthCheck.Core.Modules.GoTo.Services;
using HealthCheck.Core.Modules.LogViewer.Services;
using HealthCheck.Core.Modules.MappedData.Abstractions;
using HealthCheck.Core.Modules.MappedData.Services;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Metrics.Services;
using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;
using HealthCheck.Core.Modules.ReleaseNotes.Providers;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.Settings.Services;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Services;
using HealthCheck.Dev.Common;
using HealthCheck.Dev.Common.Comparison;
using HealthCheck.Dev.Common.Config;
using HealthCheck.Dev.Common.ContentPermutation;
using HealthCheck.Dev.Common.DataExport;
using HealthCheck.Dev.Common.Dataflow;
using HealthCheck.Dev.Common.DataMapper;
using HealthCheck.Dev.Common.DataRepeater;
using HealthCheck.Dev.Common.EndpointControl;
using HealthCheck.Dev.Common.EventNotifier;
using HealthCheck.Dev.Common.GoTo;
using HealthCheck.Dev.Common.Settings;
using HealthCheck.Dev.Common.Tests.Modules;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Services;
using HealthCheck.Module.DataExport.Storage;
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Storage;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Results;
using HealthCheck.Module.EndpointControl.Services;
using HealthCheck.Module.EndpointControl.Storage;
using HealthCheck.WebUI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace HealthCheck.DevTest.NetCore_6._0.Config
{
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

            // HC things
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
            services.AddSingleton<IHCDataRepeaterStream, TestOrderDataRepeaterStream>();
            services.AddSingleton<IHCDataRepeaterStream, TestXDataRepeaterStream>();
            services.AddSingleton<IHCDataRepeaterService, HCDataRepeaterService>();
            // Data Export
            services.AddSingleton<IHCDataExportStream, TestDataExportStreamQueryable>();
            services.AddSingleton<IHCDataExportStream, TestDataExportStreamEnumerableWithCustomParameters>();
            services.AddSingleton<IHCDataExportStream, TestDataExportStreamEnumerableWithQuery>();
            services.AddSingleton<IHCDataExportStream, TestDataExportStreamEnumerableWithQueryAndCustomParameters>();
            services.AddSingleton<IHCDataExportStream, TestDataExportStreamEnumerableWithoutInput>();
            services.AddSingleton<IHCDataExportStream, TestDataExportStreamHeavy>();
            services.AddSingleton<IHCDataExportService, HCDataExportService>();
            services.AddSingleton<IHCDataExportPresetStorage>(x => new HCFlatFileDataExportPresetStorage(@"C:\temp\DataExportPreset.json"));

            // Permutations
            services.AddSingleton<IHCContentPermutationContentHandler, DevOrderPermutationHandler>();
            services.AddSingleton<IHCContentPermutationContentHandler, DevProductPermutationHandler>();
            services.AddSingleton<IHCContentPermutationContentDiscoveryService, HCContentPermutationContentDiscoveryService>();

            // Comparison
            services.AddSingleton<IHCComparisonTypeHandler, DevOrderComparisonTypeHandler>();
            services.AddSingleton<IHCComparisonTypeHandler, DummyComparisonTypeHandler>();
            services.AddSingleton<IHCComparisonDiffer, DevOrderDifferA>();
            services.AddSingleton<IHCComparisonDiffer, DisabledByDefaultDiffer>();
            services.AddSingleton<IHCComparisonDiffer, HCComparisonDifferSerializedJson>();
            services.AddSingleton<IHCComparisonService, HCComparisonService>();

            // GoTo
            services.AddSingleton<IHCGoToResolver, PotatoGotoResolver>();
            services.AddSingleton<IHCGoToResolver, ProductGotoResolver>();
            services.AddSingleton<IHCGoToService, HCGoToService>();

            // Messages
            services.AddSingleton<IHCMessageStorage>(x => new HCFlatFileMessageStore(@"c:\temp\hc_messages"));

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
            services.AddSingleton<IHCMetricsStorage, HCMemoryMetricsStorage>();

            // MappedData
            HCMappedDataService.DisableCache = true;
            HCMappedDataModule.ExampleData = new LeftRoot();
            services.AddSingleton<IHCMappedDataService, HCMappedDataService>();

            services.AddSingleton<IHCReleaseNotesProvider>(new HCJsonFileReleaseNotesProvider(GetFilePath(@"App_Data\ReleaseNotes.json", env))
            {
                IssueUrlFactory = (id) => $"{"https://"}www.google.com/?q=Issue+{id}",
                IssueLinkTitleFactory = (id) => $"Jira {id}",
                PullRequestUrlFactory = (number) => $"{"https://"}www.google.com/?q=PR+{number}",
                //CommitShaUrlFactory = (sha) => $"{"https://"}github.com/chhjel/HealthCheck/commit/{sha}"
            });

            HCMetricsUtil.AllowTrackRequestMetrics = (r) => true;
            DevConfig.ConfigureLocalAssetUrls();
        }

        private static string GetFilePath(string relativePath, IWebHostEnvironment env)
            => Path.GetFullPath(Path.Combine(env.ContentRootPath, relativePath));

        private static void RegisterDCEServices(IServiceCollection services)
        {
            services.AddSingleton<IDynamicCodeScriptStorage>(x => new FlatFileDynamicCodeScriptStorage(@"C:\temp\DCE_scripts.json"));
        }

        private static readonly HCFlatFileStringDictionaryStorage _settingsStorage = new(@"C:\temp\settings.json");
        private static IHCSettingsService CreateSettingsService()
            => new HCDefaultSettingsService(_settingsStorage);

        private static readonly FlatFileEventSinkNotificationConfigStorage _eventSinkNotificationConfigStorage
            = new(@"c:\temp\eventconfigs.json");
        private static readonly FlatFileEventSinkKnownEventDefinitionsStorage _eventSinkNotificationDefinitionStorage
            = new(@"c:\temp\eventconfig_defs.json");
        private static IEventDataSink CreateEventDataSinkService(IServiceProvider x)
        {
            var sink = new DefaultEventDataSink(_eventSinkNotificationConfigStorage, _eventSinkNotificationDefinitionStorage)
                .AddNotifier(new HCWebHookEventNotifier())
                .AddNotifier(new MyNotifier())
                .AddNotifier(new SimpleNotifier())
                .AddPlaceholder("NOW", () => DateTimeOffset.Now.ToString())
                .AddPlaceholder("ServerName", () => Environment.MachineName)
                .SetIsEnabled(() => x.GetRequiredService<IHCSettingsService>().GetSettings<TestSettings>().EnableEventRegistering);
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
                UnifiedSearches = new IHCDataflowUnifiedSearch<RuntimeTestAccessRole>[]
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
}
