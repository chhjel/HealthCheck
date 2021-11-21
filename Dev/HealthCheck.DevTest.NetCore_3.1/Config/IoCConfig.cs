using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Services;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Modules.Dataflow.Services;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Services;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Notifiers;
using HealthCheck.Core.Modules.EventNotifications.Services;
using HealthCheck.Core.Modules.LogViewer.Services;
using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Metrics.Services;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.Settings.Services;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Services;
using HealthCheck.Dev.Common;
using HealthCheck.Dev.Common.Dataflow;
using HealthCheck.Dev.Common.DataRepeater;
using HealthCheck.Dev.Common.EventNotifier;
using HealthCheck.Dev.Common.Settings;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Services;
using HealthCheck.Module.EndpointControl.Storage;
using HealthCheck.WebUI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace HealthCheck.DevTest.NetCore_3._1.Config
{
    public static class IoCConfig
    {
        internal static void Configure(IServiceCollection services, IWebHostEnvironment env)
        {
            // .Net Core things
            services.AddHttpContextAccessor();

            // HC things
            services.AddSingleton<IEndpointControlRuleStorage>((x) => new FlatFileEndpointControlRuleStorage(GetFilePath(@"App_Data\ec_rules.json", env)));
            services.AddSingleton<IEndpointControlEndpointDefinitionStorage>((x) => new FlatFileEndpointControlEndpointDefinitionStorage(GetFilePath(@"App_Data\ec_defs.json", env)));
            services.AddSingleton<IEndpointControlRequestHistoryStorage>((x) => new FlatFileEndpointControlRequestHistoryStorage(GetFilePath(@"App_Data\ec_history.json", env)));
            services.AddSingleton<IEndpointControlService, DefaultEndpointControlService>();

            services.AddSingleton<IHCDataRepeaterStream, TestOrderDataRepeaterStream>();
            services.AddSingleton<IHCDataRepeaterStream, TestXDataRepeaterStream>();
            services.AddSingleton<IHCDataRepeaterService, HCDataRepeaterService>();
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

            HCMetricsUtil.AllowTrackRequestMetrics = (r) => true;
        }

        private static string GetFilePath(string relativePath, IWebHostEnvironment env)
            => Path.GetFullPath(Path.Combine(env.ContentRootPath, relativePath));

        private static readonly HCFlatFileStringDictionaryStorage _settingsStorage = new HCFlatFileStringDictionaryStorage(@"C:\temp\settings.json");
        private static IHCSettingsService CreateSettingsService()
            => new HCDefaultSettingsService(_settingsStorage);

        private static readonly FlatFileEventSinkNotificationConfigStorage _eventSinkNotificationConfigStorage
            = new FlatFileEventSinkNotificationConfigStorage(@"c:\temp\eventconfigs.json");
        private static readonly FlatFileEventSinkKnownEventDefinitionsStorage _eventSinkNotificationDefinitionStorage
            = new FlatFileEventSinkKnownEventDefinitionsStorage(@"c:\temp\eventconfig_defs.json");
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

        public static readonly TestStreamA TestStreamA = new TestStreamA();
        private static readonly TestStreamB _testStreamB = new TestStreamB();
        private static readonly TestStreamC _testStreamC = new TestStreamC();
        private static readonly SimpleStream _simpleStream = new SimpleStream("Simple A");
        private static readonly TestMemoryStream _memoryStream = new TestMemoryStream("Memory");
        private static readonly TestMemoryStream _otherStream1 = new TestMemoryStream(null);
        private static readonly TestMemoryStream _otherStream2 = new TestMemoryStream(null);
        private static IDataflowService<RuntimeTestAccessRole> CreateDataflowService()
        {
            return new DefaultDataflowService<RuntimeTestAccessRole>(new DefaultDataflowServiceOptions<RuntimeTestAccessRole>()
            {
                Streams = new IDataflowStream<RuntimeTestAccessRole>[]
                {
                    TestStreamA,
                    _testStreamB,
                    _testStreamC,
                    _simpleStream,
                    _memoryStream,
                    _otherStream1,
                    _otherStream2
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
